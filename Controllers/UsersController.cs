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
        private readonly BLOGAURA.Data.ApplicationDbContext _context;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            IFollowService followService,
            BLOGAURA.Data.ApplicationDbContext context)
        {
            _userManager = userManager;
            _followService = followService;
            _context = context;
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

        [HttpGet]
        public async Task<IActionResult> Followers(int userId)
        {
            var profile = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (profile == null)
            {
                return NotFound();
            }

            var users = await _context.UserFollows
                .Include(f => f.Follower)
                .Where(f => f.FollowedId == userId)
                .Select(f => new BLOGAURA.Models.Social.FollowUserItem
                {
                    UserId = f.Follower.Id,
                    DisplayName = f.Follower.DisplayName ?? f.Follower.UserName ?? "Utilisateur",
                    Username = f.Follower.UserName ?? string.Empty,
                    PhotoUrl = f.Follower.PhotoUrl
                })
                .ToListAsync();

            var vm = new BLOGAURA.Models.Social.FollowListViewModel
            {
                ProfileUserId = profile.Id.ToString(),
                ProfileDisplayName = profile.DisplayName ?? profile.UserName ?? "Utilisateur",
                IsFollowers = true,
                Users = users
            };

            return View("FollowList", vm);
        }

        [HttpGet]
        public async Task<IActionResult> Following(int userId)
        {
            var profile = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (profile == null)
            {
                return NotFound();
            }

            var users = await _context.UserFollows
                .Include(f => f.Followed)
                .Where(f => f.FollowerId == userId)
                .Select(f => new BLOGAURA.Models.Social.FollowUserItem
                {
                    UserId = f.Followed.Id,
                    DisplayName = f.Followed.DisplayName ?? f.Followed.UserName ?? "Utilisateur",
                    Username = f.Followed.UserName ?? string.Empty,
                    PhotoUrl = f.Followed.PhotoUrl
                })
                .ToListAsync();

            var vm = new BLOGAURA.Models.Social.FollowListViewModel
            {
                ProfileUserId = profile.Id.ToString(),
                ProfileDisplayName = profile.DisplayName ?? profile.UserName ?? "Utilisateur",
                IsFollowers = false,
                Users = users
            };

            return View("FollowList", vm);
        }
    }
}
