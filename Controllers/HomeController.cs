using System.Diagnostics;
using System.Security.Claims;
using BLOGAURA.Data;
using BLOGAURA.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BLOGAURA.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly BlogContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public HomeController(ILogger<HomeController> logger, BlogContext context, IPasswordHasher<User> passwordHasher)
        {
            _logger = logger;
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult Profile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return RedirectToAction("Login");
            }

            var id = int.Parse(userIdClaim.Value);
            var user = _context.User.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            return View(user);
        }

        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated ?? false)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(User model)
        {
            if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Email and password are required.");
                return View(model);
            }

            var user = _context.User.FirstOrDefault(u => u.Email == model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(model);
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.PasswordHash);
            if (result == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated ?? false)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(User model, string ConfirmPassword, DateTime DateOfBirth)
        {
            if (string.IsNullOrWhiteSpace(model.Name) || string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "All fields are required.");
                return View(model);
            }

            if (model.PasswordHash != ConfirmPassword)
            {
                ModelState.AddModelError(string.Empty, "Passwords do not match.");
                return View(model);
            }

            if (_context.User.Any(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email is already in use.");
                return View(model);
            }

            var user = new User
            {
                Name = model.Name,
                Email = model.Email,
                Role = "User",
                CreatedAt = DateTime.Now
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, model.PasswordHash);

            _context.User.Add(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Registration successful. Please log in.";
            return RedirectToAction("Login", "Home");
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
