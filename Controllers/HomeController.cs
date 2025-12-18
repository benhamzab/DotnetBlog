using System.Diagnostics;
using System.Security.Claims;
using BLOGAURA.Data;
using BLOGAURA.Models;
using BLOGAURA.Models.Auth;
using BLOGAURA.Models.Posts;
using BLOGAURA.Services.Posts;
using BLOGAURA.Services.Social;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BLOGAURA.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IPostService _postService;
        private readonly ApplicationDbContext _authContext;
        private readonly IFollowService _followService;

        public HomeController(ILogger<HomeController> logger, IPostService postService, ApplicationDbContext authContext, IFollowService followService)
        {
            _logger = logger;
            _postService = postService;
            _authContext = authContext;
            _followService = followService;
        }

        public async Task<IActionResult> Index()
        {
            if (!(User.Identity?.IsAuthenticated ?? false))
            {
                return RedirectToAction("Login", "Auth");
            }

            ViewData["UserName"] = User.Identity?.Name;

            // Load latest posts for the home feed
            var posts = await _postService.GetLatestPostsAsync(20);
            
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
            {
                var user = await _authContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user != null)
                {
                    var postsCount = await _authContext.Posts.CountAsync(p => p.UserId == user.Id);
                    var followingCount = await _authContext.UserFollows.CountAsync(f => f.FollowerId == user.Id);
                    var followersCount = await _authContext.UserFollows.CountAsync(f => f.FollowedId == user.Id);

                    var profileCard = new BLOGAURA.Models.Auth.ProfileCardViewModel
                    {
                        UserId = user.Id.ToString(),
                        DisplayName = user.DisplayName ?? user.UserName ?? "Utilisateur",
                        Email = user.Email ?? string.Empty,
                        RoleLabel = "Membre BlogAura",
                        PostsCount = postsCount,
                        FollowingCount = followingCount,
                        FollowersCount = followersCount,
                        PhotoUrl = user.PhotoUrl
                    };

                    ViewBag.ProfileCard = profileCard;
                }
            }

            // Build post card view models with edit/delete capability
            var canAdmin = User.IsInRole("Admin");
            var postCards = posts.Select(p => {
                var canOwner = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier) != null &&
                               int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value, out var uid) &&
                               uid == p.UserId;
                return new BLOGAURA.Models.Posts.PostCardViewModel
                {
                    Id = p.Id,
                    AuthorId = p.UserId.ToString(),
                    AuthorName = p.User?.DisplayName ?? p.User?.UserName ?? "Utilisateur",
                    AuthorPhotoUrl = p.User?.ProfilePictureUrl ?? string.Empty,
                    CreatedAtFormatted = p.CreatedAt.ToLocalTime().ToString("g"),
                    CanEditOrDelete = canAdmin || canOwner,
                    CanDelete = canAdmin || canOwner,
                    Post = p
                };
            }).ToList();

            return View(postCards);
        }


        [Authorize]
        public async Task<IActionResult> Profile(int? userId)
        {
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (currentUserIdClaim == null || !int.TryParse(currentUserIdClaim.Value, out var currentUserId))
            {
                return RedirectToAction("Login", "Auth");
            }

            // If no userId specified, show current user's profile
            var targetUserId = userId ?? currentUserId;

            var user = await _authContext.Users.FirstOrDefaultAsync(u => u.Id == targetUserId);
            if (user == null)
            {
                return NotFound();
            }

            var posts = await _postService.GetUserPostsAsync(targetUserId);
            
            // Get follow data
            var isFollowing = false;
            if (targetUserId != currentUserId)
            {
                isFollowing = await _followService.IsFollowingAsync(currentUserId, targetUserId);
            }

            var followersCount = await _followService.GetFollowersCountAsync(targetUserId);
            var followingCount = await _followService.GetFollowingCountAsync(targetUserId);

            // Build feed lists for X-style tabs
            var repliesPosts = await _authContext.Comments
                .Include(c => c.Post).ThenInclude(p => p.User)
                .Where(c => c.UserId == targetUserId)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => c.Post)
                .Distinct()
                .ToListAsync();

            var repostsPosts = await _authContext.Reposts
                .Include(r => r.OriginalPost).ThenInclude(p => p.User)
                .Where(r => r.UserId == targetUserId)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => r.OriginalPost)
                .ToListAsync();

            var likesPosts = await _authContext.PostLikes
                .Include(l => l.Post).ThenInclude(p => p.User)
                .Where(l => l.UserId == targetUserId)
                .OrderByDescending(l => l.CreatedAt)
                .Select(l => l.Post)
                .ToListAsync();

            var userReels = await _authContext.Reels
                .Include(r => r.User)
                .Include(r => r.Likes)
                .Include(r => r.Comments)
                .Where(r => r.UserId == targetUserId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var vm = new ProfileViewModel
            {
                User = user,
                ProfileUserId = user.Id.ToString(),
                CurrentUserId = currentUserId.ToString(),
                DisplayName = user.DisplayName ?? user.UserName ?? "Utilisateur",
                Username = user.UserName ?? string.Empty,
                PhotoUrl = user.PhotoUrl,
                Bio = user.About,
                PostsCount = posts.Count,
                FollowersCount = followersCount,
                FollowingCount = followingCount,
                Posts = posts,
                Replies = repliesPosts,
                Reposts = repostsPosts,
                Likes = likesPosts,
                Reels = userReels,
                IsFollowing = isFollowing
            };

            ViewBag.IsOwnProfile = (targetUserId == currentUserId);
            try
            {
                if (User.IsInRole("Admin") || User.IsInRole("Editor"))
                {
                    var upcoming = await _authContext.ContentCalendar
                        .Where(c => c.EditorUserId == currentUserId && c.PlannedPublishDate.Date >= DateTime.UtcNow.Date)
                        .OrderBy(c => c.PlannedPublishDate)
                        .Take(5)
                        .ToListAsync();
                    ViewBag.NextScheduledContents = upcoming;
                }
            }
            catch { }
            if (targetUserId == currentUserId)
            {
                ViewBag.SettingsModel = new BLOGAURA.Models.Auth.EditProfileSettingsViewModel
                {
                    DisplayName = user.DisplayName,
                    Email = user.Email,
                    UserName = user.UserName,
                    CurrentPhotoUrl = user.ProfilePictureUrl,
                    ProfileUserId = user.Id
                };
            }

            return View(vm);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAbout(UpdateAboutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Erreur de validation.";
                return RedirectToAction("Profile");
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = await _authContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            user.About = model.About?.Trim();
            await _authContext.SaveChangesAsync();

            TempData["Success"] = "Votre description a été mise à jour avec succès.";
            return RedirectToAction("Profile");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}
