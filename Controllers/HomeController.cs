using System.Diagnostics;
using System.Security.Claims;
using BLOGAURA.Data;
using BLOGAURA.Models;
using BLOGAURA.Models.Auth;
using BLOGAURA.Models.Posts;
using BLOGAURA.Services.Posts;
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

        public HomeController(ILogger<HomeController> logger, IPostService postService, ApplicationDbContext authContext)
        {
            _logger = logger;
            _postService = postService;
            _authContext = authContext;
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
        public async Task<IActionResult> Profile()
        {
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

            var posts = await _postService.GetUserPostsAsync(userId);
            var vm = new ProfileViewModel
            {
                User = user,
                Posts = posts
            };

            return View(vm);
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
