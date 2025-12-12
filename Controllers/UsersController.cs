using System.Security.Claims;
using BLOGAURA.Models.Auth;
using BLOGAURA.Services.Social;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BLOGAURA.Controllers
{
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFollowService _followService;

        public UsersController(UserManager<ApplicationUser> userManager, IFollowService followService)
        {
            _userManager = userManager;
            _followService = followService;
        }

        [HttpGet]
        public async Task<IActionResult> Search(string? query)
        {
            var model = new UserSearchViewModel
            {
                Query = query
            };

            if (!string.IsNullOrWhiteSpace(query))
            {
                var searchTerm = query.ToLower();
                
                var users = await _userManager.Users
                    .Where(u => 
                        u.UserName!.ToLower().Contains(searchTerm) ||
                        (u.Email != null && u.Email.ToLower().Contains(searchTerm)) ||
                        (u.About != null && u.About.ToLower().Contains(searchTerm)))
                    .Take(20)
                    .ToListAsync();

                model.Results = users.Select(u => new UserSummary
                {
                    Id = u.Id,
                    UserName = u.UserName ?? "Utilisateur",
                    Email = u.Email,
                    ProfilePictureUrl = u.ProfilePictureUrl,
                    About = u.About
                }).ToList();
            }

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Follow(int userId)
        {
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (currentUserIdClaim == null || !int.TryParse(currentUserIdClaim.Value, out var currentUserId))
            {
                return RedirectToAction("Login", "Auth");
            }

            // Prevent self-follow
            if (currentUserId == userId)
            {
                TempData["Error"] = "Vous ne pouvez pas vous suivre vous-même.";
                return RedirectToAction("Profile", "Home", new { userId });
            }

            try
            {
                await _followService.FollowAsync(currentUserId, userId);
                TempData["Success"] = "Vous suivez maintenant cet utilisateur.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Une erreur est survenue.";
                Console.WriteLine($"Error following user: {ex.Message}");
            }

            return RedirectToAction("Profile", "Home", new { userId });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unfollow(int userId)
        {
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (currentUserIdClaim == null || !int.TryParse(currentUserIdClaim.Value, out var currentUserId))
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                await _followService.UnfollowAsync(currentUserId, userId);
                TempData["Success"] = "Vous ne suivez plus cet utilisateur.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Une erreur est survenue.";
                Console.WriteLine($"Error unfollowing user: {ex.Message}");
            }

            return RedirectToAction("Profile", "Home", new { userId });
        }
    }
}
