using BLOGAURA.Data;
using BLOGAURA.Models.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BLOGAURA.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<ApplicationUser> _passwordHasher;

        public AuthService(ApplicationDbContext context, IPasswordHasher<ApplicationUser> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<ApplicationUser?> CreateUserAsync(RegisterModel model)
        {
            var existing = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == model.Email || u.Username == model.Username);
            if (existing != null)
            {
                return null;
            }

            var user = new ApplicationUser
            {
                Username = model.Username,
                Email = model.Email,
                CreatedAt = DateTime.UtcNow
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }

        public Task<ApplicationUser?> GetUserByEmailAsync(string email)
        {
            return _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<ApplicationUser?> ValidateUserAsync(LoginModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Password))
            {
                return null;
            }

            ApplicationUser? user = null;

            if (!string.IsNullOrWhiteSpace(model.Email))
            {
                user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            }
            else if (!string.IsNullOrWhiteSpace(model.Username))
            {
                user = await _context.Users.FirstOrDefaultAsync(u => u.Username == model.Username);
            }

            if (user == null)
            {
                return null;
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);
            if (result == PasswordVerificationResult.Failed)
            {
                return null;
            }

            return user;
        }
    }
}
