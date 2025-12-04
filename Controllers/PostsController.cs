using System.Security.Claims;
using System.IO;
using System.Linq;
using BLOGAURA.Models.Posts;
using BLOGAURA.Services.Posts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BLOGAURA.Controllers
{
    [Authorize]
    public class PostsController : Controller
    {
        private readonly IPostService _postService;

        public PostsController(IPostService postService)
        {
            _postService = postService;
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

            await _postService.CreatePostAsync(userId, model);
            TempData["PostSuccess"] = "Votre post a été publié avec succès.";

            return RedirectToAction("Profile", "Home");
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

            await _postService.CreatePostAsync(userId, model);

            return RedirectToAction("Index", "Home");
        }
    }
}
