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
using BLOGAURA.Models;

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
            return View(posts);
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

            var user = _authContext.Users.FirstOrDefault(u => u.Id == targetUserId);
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

            var vm = new ProfileViewModel
            {
                User = user,
                Posts = posts,
                IsFollowing = isFollowing,
                FollowersCount = followersCount,
                FollowingCount = followingCount
            };

            ViewBag.IsOwnProfile = (targetUserId == currentUserId);

            return View(vm);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateAbout(UpdateAboutViewModel model)
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

            var user = _authContext.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            user.About = model.About?.Trim();
            _authContext.SaveChanges();

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
