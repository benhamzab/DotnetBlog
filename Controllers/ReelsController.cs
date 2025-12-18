using System.Security.Claims;
using BLOGAURA.Data;
using BLOGAURA.Models.Reels;
using BLOGAURA.Models.Posts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BLOGAURA.Controllers
{
    [Authorize]
    public class ReelsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ReelsController> _logger;

        public ReelsController(ApplicationDbContext context, IWebHostEnvironment environment, ILogger<ReelsController> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? campus, string? category)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var currentUserId = (userIdClaim != null && int.TryParse(userIdClaim.Value, out var uid)) ? uid : (int?)null;

            var query = _context.Reels
                .Include(r => r.User)
                .Include(r => r.EventPost)
                .Include(r => r.Likes)
                .Include(r => r.Comments)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(campus))
            {
                query = query.Where(r => (r.User.City != null && r.User.City.Contains(campus)) || (r.User.University != null && r.User.University.Contains(campus)));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(r => r.Category != null && r.Category.Contains(category));
            }

            var reels = await query.OrderByDescending(r => r.CreatedAt).ToListAsync();

            var vms = reels.Select(r => new ReelViewModel
            {
                Id = r.Id,
                UserId = r.UserId.ToString(),
                UserName = r.User?.DisplayName ?? r.User?.UserName ?? "Utilisateur",
                UserPhotoUrl = r.User?.PhotoUrl ?? r.User?.ProfilePictureUrl ?? string.Empty,
                Caption = r.Caption,
                VideoPath = r.VideoPath,
                CreatedAtFormatted = r.CreatedAt.ToLocalTime().ToString("g"),
                LikesCount = r.Likes?.Count ?? 0,
                CommentsCount = r.Comments?.Count ?? 0,
                IsLikedByCurrentUser = currentUserId.HasValue && (r.Likes?.Any(l => l.UserId == currentUserId.Value) ?? false),
                EventPostId = r.EventPostId,
                EventTitle = r.EventPost?.Title ?? string.Empty,
                Category = r.Category
            }).ToList();

            ViewBag.FilterCampus = campus;
            ViewBag.FilterCategory = category;
            return View(vms);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var posts = await _context.Posts.OrderByDescending(p => p.CreatedAt).Take(50).ToListAsync();
            ViewBag.Posts = posts;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateReelViewModel model)
        {
            _logger.LogInformation("[Reels] POST Create reached. ModelState valid: {Valid}", ModelState.IsValid);
            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                _logger.LogWarning("[Reels] Create invalid ModelState. Errors: {Errors}", errors);
                var posts = await _context.Posts.OrderByDescending(p => p.CreatedAt).Take(50).ToListAsync();
                ViewBag.Posts = posts;
                return View(model);
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            if (model.VideoFile == null || model.VideoFile.Length == 0)
            {
                ModelState.AddModelError("VideoFile", "La vidéo est requise.");
                var posts = await _context.Posts.OrderByDescending(p => p.CreatedAt).Take(50).ToListAsync();
                ViewBag.Posts = posts;
                return View(model);
            }

            var ext = Path.GetExtension(model.VideoFile.FileName).ToLowerInvariant();
            var allowed = new[] { ".mp4", ".mov", ".webm" };
            if (!allowed.Contains(ext))
            {
                ModelState.AddModelError("VideoFile", "Format vidéo non pris en charge.");
                var posts = await _context.Posts.OrderByDescending(p => p.CreatedAt).Take(50).ToListAsync();
                ViewBag.Posts = posts;
                return View(model);
            }

            var root = _environment.WebRootPath ?? string.Empty;
            if (string.IsNullOrEmpty(root))
            {
                throw new InvalidOperationException("WebRootPath is not configured.");
            }

            var uploads = Path.Combine(root, "uploads", "reels");
            Directory.CreateDirectory(uploads);

            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploads, fileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await model.VideoFile.CopyToAsync(stream);
            }

            var reel = new Reel
            {
                UserId = userId,
                EventPostId = model.EventPostId,
                Caption = model.Caption?.Trim() ?? string.Empty,
                VideoPath = $"/uploads/reels/{fileName}",
                ThumbnailPath = null,
                Category = model.Category,
                CreatedAt = DateTime.UtcNow
            };

            _context.Reels.Add(reel);
            await _context.SaveChangesAsync();
            _logger.LogInformation("[Reels] Created Reel {ReelId} by User {UserId}", reel.Id, userId);

            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLike(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var reel = await _context.Reels.Include(r => r.Likes).FirstOrDefaultAsync(r => r.Id == id);
            if (reel == null)
            {
                return NotFound();
            }

            var existing = await _context.ReelLikes.SingleOrDefaultAsync(l => l.ReelId == id && l.UserId == userId);
            if (existing != null)
            {
                _context.ReelLikes.Remove(existing);
            }
            else
            {
                _context.ReelLikes.Add(new ReelLike
                {
                    ReelId = id,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer)) return Redirect(referer);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(CreateReelCommentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var refererInvalid = Request.Headers["Referer"].ToString();
                if (!string.IsNullOrEmpty(refererInvalid)) return Redirect(refererInvalid);
                return RedirectToAction("Index");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var exists = await _context.Reels.AnyAsync(r => r.Id == model.ReelId);
            if (!exists)
            {
                return NotFound();
            }

            _context.ReelComments.Add(new ReelComment
            {
                ReelId = model.ReelId,
                UserId = userId,
                Content = model.Content.Trim(),
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer)) return Redirect(referer);
            return RedirectToAction("Index");
        }
    }
}
