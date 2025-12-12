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
                await _postService.CreatePostAsync(userId, model);
                return RedirectToAction("Index", "Home");
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
