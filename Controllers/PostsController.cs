using System.Security.Claims;
using System.IO;
using System.Linq;
using BLOGAURA.Models.Posts;
using BLOGAURA.Services.Posts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BLOGAURA.Controllers
{
    [Authorize]
    public class PostsController : Controller
    {
        private readonly IPostService _postService;
        private readonly BLOGAURA.Data.ApplicationDbContext _context;

        public PostsController(IPostService postService, BLOGAURA.Data.ApplicationDbContext context)
        {
            _postService = postService;
            _context = context;
        }

        [HttpGet]
        public IActionResult CreatePost()
        {
            return View("Create", new CreatePostModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost(CreatePostModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Create", model);
            }

            // Optional: validate images count / size here if needed

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                await _postService.CreatePostAsync(userId, model);
                TempData["PostSuccess"] = "Votre post a été publié avec succès.";
                return RedirectToAction("Profile", "Home");
            }
            catch (Exception ex)
            {
                // Log the exception (in production, use ILogger)
                Console.WriteLine($"Error creating post: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                ModelState.AddModelError(string.Empty, "Une erreur est survenue lors de la création du post. Veuillez réessayer.");
                return View("Create", model);
            }
        }

        private void ValidateMediaFile(CreatePostModel model, string keyPrefix)
        {
            var file = model.MediaFile ?? model.ImageFile ?? model.VideoFile;
            if (file == null || file.Length == 0)
            {
                return; // optional
            }

            const long maxBytes = 20L * 1024 * 1024; // 20 MB
            if (file.Length > maxBytes)
            {
                ModelState.AddModelError(keyPrefix, "Le fichier ne doit pas dépasser 20 Mo.");
                return;
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".mp4", ".mov", ".avi" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError(keyPrefix, "Type de fichier non pris en charge. Formats autorisés : .jpg, .jpeg, .png, .webp, .mp4, .mov, .avi.");
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            // Preload planned calendar items for this editor
            var items = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>();
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
            {
                try
                {
                    var now = DateTime.UtcNow.Date;
                    var cutoff = now.AddDays(42);
                    var planned = _context.ContentCalendar
                        .Where(c => c.EditorUserId == userId && (c.Status == "Planned" || c.Status == "InProgress") && c.PlannedPublishDate.Date <= cutoff)
                        .OrderBy(c => c.PlannedPublishDate)
                        .Take(50)
                        .Select(c => new { c.Id, c.Title, c.PlannedPublishDate, c.ContentType })
                        .ToList();
                    items = planned.Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = $"{c.PlannedPublishDate:yyyy-MM-dd} • {c.ContentType} • {c.Title}"
                    }).ToList();
                }
                catch { }
            }

            ViewBag.PlannedItems = items;
            return View(new CreatePostModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePostModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Optional: validate images count / size here if needed

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var post = await _postService.CreatePostAsync(userId, model);

                // Link post to selected content calendar item
                if (model.ContentCalendarItemId.HasValue)
                {
                    var item = await _context.ContentCalendar.FirstOrDefaultAsync(c => c.Id == model.ContentCalendarItemId.Value && c.EditorUserId == userId);
                    if (item != null)
                    {
                        item.PostId = post.Id;
                        item.Status = "Published";
                        await _context.SaveChangesAsync();
                    }
                }

                return RedirectToAction("Details", new { id = post.Id });
            }
            catch (Exception ex)
            {
                // Log the exception (in production, use ILogger)
                Console.WriteLine($"Error creating post: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                ModelState.AddModelError(string.Empty, "Une erreur est survenue lors de la création du post. Veuillez réessayer.");
                return View(model);
            }
        }

        // GET: Posts/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var post = await _postService.GetPostByIdAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            // Check authorization - only post owner can edit
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            if (post.UserId != userId)
            {
                TempData["Error"] = "Vous n'êtes pas autorisé à modifier ce post.";
                return RedirectToAction("Index", "Home");
            }

            var model = new EditPostViewModel
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                ExistingImagePath = post.ImagePath
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var post = await _postService.GetPostByIdAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            var vm = new BLOGAURA.Models.Posts.PostDetailsViewModel
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                CreatedAtFormatted = post.CreatedAt.ToLocalTime().ToString("g"),
                AuthorId = post.UserId.ToString(),
                AuthorName = post.User?.DisplayName ?? post.User?.UserName ?? "Utilisateur",
                AuthorPhotoUrl = post.User?.PhotoUrl ?? post.User?.ProfilePictureUrl ?? string.Empty,
                LikeCount = post.Likes?.Count ?? 0,
                ImagePaths = (post.Images ?? new List<BLOGAURA.Models.Posts.PostImage>())
                    .Select(i => i.ImagePath)
                    .Where(p => !string.IsNullOrEmpty(p))
                    .ToList(),
                Comments = (post.Comments ?? new List<BLOGAURA.Models.Posts.Comment>())
                    .OrderByDescending(c => c.CreatedAt)
                    .Select(c => new BLOGAURA.Models.Posts.PostDetailsViewModel.CommentItem
                    {
                        AuthorId = c.UserId.ToString(),
                        AuthorName = c.User?.DisplayName ?? c.User?.UserName ?? "Utilisateur",
                        AuthorPhotoUrl = c.User?.PhotoUrl ?? c.User?.ProfilePictureUrl ?? string.Empty,
                        Content = c.Content,
                        CreatedAtFormatted = c.CreatedAt.ToLocalTime().ToString("g")
                    }).ToList()
            };

            try
            {
                var relatedPlanned = await _context.ContentCalendar
                    .Where(c => c.EventId == post.Id)
                    .OrderBy(c => c.PlannedPublishDate)
                    .ToListAsync();
                ViewBag.RelatedPlanned = relatedPlanned;
                var relatedReels = await _context.Reels
                    .Include(r => r.User)
                    .Where(r => r.EventPostId == post.Id)
                    .OrderByDescending(r => r.CreatedAt)
                    .Take(5)
                    .ToListAsync();
                ViewBag.RelatedReels = relatedReels;
            }
            catch { }

            return View(vm);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLike(int postId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var exists = await _context.Posts.AnyAsync(p => p.Id == postId);
            if (!exists)
            {
                return NotFound();
            }

            var existing = await _context.PostLikes.SingleOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);
            if (existing != null)
            {
                _context.PostLikes.Remove(existing);
            }
            else
            {
                _context.PostLikes.Add(new BLOGAURA.Models.Posts.PostLike
                {
                    PostId = postId,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();

            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer))
            {
                return Redirect(referer);
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(BLOGAURA.Models.Posts.CreateCommentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Details", new { id = model.PostId });
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == model.PostId);
            if (post == null)
            {
                return NotFound();
            }

            var comment = new BLOGAURA.Models.Posts.Comment
            {
                PostId = model.PostId,
                UserId = userId,
                Content = model.Content.Trim(),
                CreatedAt = DateTime.UtcNow
            };
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = model.PostId });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Repost(int postId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == postId);
            if (post == null)
            {
                return NotFound();
            }

            var existing = await _context.Reposts.SingleOrDefaultAsync(r => r.OriginalPostId == postId && r.UserId == userId);
            if (existing == null)
            {
                _context.Reposts.Add(new BLOGAURA.Models.Posts.Repost
                {
                    OriginalPostId = postId,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index", "Home");
        }

        // POST: Posts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditPostViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var post = await _postService.GetPostByIdAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            // Check authorization - only post owner can edit
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            if (post.UserId != userId)
            {
                TempData["Error"] = "Vous n'êtes pas autorisé à modifier ce post.";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                await _postService.UpdatePostAsync(post, model, HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>());
                TempData["Success"] = "Post modifié avec succès.";
                return RedirectToAction("Profile", "Home");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating post: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                ModelState.AddModelError(string.Empty, "Une erreur est survenue lors de la modification du post.");
                return View(model);
            }
        }

        // GET: Posts/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var post = await _postService.GetPostByIdAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            // Check authorization - only post owner can delete
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            if (post.UserId != userId)
            {
                TempData["Error"] = "Vous n'êtes pas autorisé à supprimer ce post.";
                return RedirectToAction("Index", "Home");
            }

            return View(post);
        }

        // POST: Posts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _postService.GetPostByIdAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            // Check authorization - only post owner can delete
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            if (post.UserId != userId)
            {
                TempData["Error"] = "Vous n'êtes pas autorisé à supprimer ce post.";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                await _postService.DeletePostAsync(post, HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>());
                TempData["Success"] = "Post supprimé avec succès.";
                return RedirectToAction("Profile", "Home");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting post: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                TempData["Error"] = "Une erreur est survenue lors de la suppression du post.";
                return RedirectToAction("Profile", "Home");
            }
        }
    }
}
