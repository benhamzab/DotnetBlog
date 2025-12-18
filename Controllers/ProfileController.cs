using System.Security.Claims;
using BLOGAURA.Data;
using BLOGAURA.Models.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BLOGAURA.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public ProfileController(UserManager<ApplicationUser> userManager, IWebHostEnvironment environment, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _environment = environment;
            _signInManager = signInManager;
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var model = new EditProfileViewModel
            {
                About = user.About,
                University = user.University,
                Grade = user.Grade,
                City = user.City,
                Age = user.Age,
                Hobbies = user.Hobbies,
                FavoriteEventTypes = user.FavoriteEventTypes,
                LearningNow = user.LearningNow,
                GithubUrl = user.GithubUrl,
                LinkedinUrl = user.LinkedinUrl,
                PortfolioUrl = user.PortfolioUrl
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                // Update all student profile fields
                user.About = model.About?.Trim();
                user.University = model.University?.Trim();
                user.Grade = model.Grade?.Trim();
                user.City = model.City?.Trim();
                user.Age = model.Age;
                user.Hobbies = model.Hobbies?.Trim();
                user.FavoriteEventTypes = model.FavoriteEventTypes?.Trim();
                user.LearningNow = model.LearningNow?.Trim();
                user.GithubUrl = model.GithubUrl?.Trim();
                user.LinkedinUrl = model.LinkedinUrl?.Trim();
                user.PortfolioUrl = model.PortfolioUrl?.Trim();

                if (model.ProfileImage != null && model.ProfileImage.Length > 0)
                {
                    var root = _environment.WebRootPath;
                    if (string.IsNullOrEmpty(root))
                    {
                        throw new InvalidOperationException("WebRootPath is not configured.");
                    }

                    var uploads = Path.Combine(root, "uploads", "profiles");
                    Directory.CreateDirectory(uploads);

                    if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
                    {
                        var oldPath = Path.Combine(root, user.ProfilePictureUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldPath))
                        {
                            try { System.IO.File.Delete(oldPath); } catch { }
                        }
                    }

                    var ext = Path.GetExtension(model.ProfileImage.FileName).ToLowerInvariant();
                    var name = $"{Guid.NewGuid()}{ext}";
                    var path = Path.Combine(uploads, name);
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await model.ProfileImage.CopyToAsync(stream);
                    }
                    user.ProfilePictureUrl = $"/uploads/profiles/{name}";
                }

                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(model);
                }

                TempData["Success"] = "Votre profil a été mis à jour avec succès.";
                return RedirectToAction("Profile", "Home");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating profile: {ex.Message}");
                ModelState.AddModelError(string.Empty, "Une erreur est survenue lors de la mise à jour du profil.");
                return View(model);
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Settings(int? userId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (userId.HasValue && userId.Value != user.Id)
            {
                return Forbid();
            }

            var vm = new BLOGAURA.Models.Auth.EditProfileSettingsViewModel
            {
                DisplayName = user.DisplayName,
                Email = user.Email,
                UserName = user.UserName,
                CurrentPhotoUrl = user.ProfilePictureUrl,
                ProfileUserId = user.Id
            };

            return View(vm);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Settings(BLOGAURA.Models.Auth.EditProfileSettingsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (model.ProfileUserId.HasValue && model.ProfileUserId.Value != user.Id)
            {
                return Forbid();
            }

            user.DisplayName = model.DisplayName?.Trim();

            if (!string.IsNullOrWhiteSpace(model.Email) && !string.Equals(user.Email, model.Email, StringComparison.OrdinalIgnoreCase))
            {
                var setEmailResult = await _userManager.SetEmailAsync(user, model.Email);
                if (!setEmailResult.Succeeded)
                {
                    foreach (var error in setEmailResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(model);
                }
            }

            if (!string.IsNullOrWhiteSpace(model.UserName) && !string.Equals(user.UserName, model.UserName, StringComparison.OrdinalIgnoreCase))
            {
                var setUserNameResult = await _userManager.SetUserNameAsync(user, model.UserName);
                if (!setUserNameResult.Succeeded)
                {
                    foreach (var error in setUserNameResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(model);
                }
            }

            if (model.NewProfilePhoto != null && model.NewProfilePhoto.Length > 0)
            {
                var root = _environment.WebRootPath;
                if (string.IsNullOrEmpty(root))
                {
                    throw new InvalidOperationException("WebRootPath is not configured.");
                }

                var uploads = Path.Combine(root, "uploads", "profiles");
                Directory.CreateDirectory(uploads);

                if (!string.IsNullOrEmpty(user.ProfilePictureUrl))
                {
                    var oldPath = Path.Combine(root, user.ProfilePictureUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath))
                    {
                        try { System.IO.File.Delete(oldPath); } catch { }
                    }
                }

                var ext = Path.GetExtension(model.NewProfilePhoto.FileName).ToLowerInvariant();
                var name = $"{Guid.NewGuid()}{ext}";
                var path = Path.Combine(uploads, name);
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await model.NewProfilePhoto.CopyToAsync(stream);
                }
                user.ProfilePictureUrl = $"/uploads/profiles/{name}";
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            await _signInManager.RefreshSignInAsync(user);

            return RedirectToAction("Profile", "Home", new { userId = user.Id });
        }
    }
}
