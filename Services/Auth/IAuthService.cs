using BLOGAURA.Models.Auth;

namespace BLOGAURA.Services.Auth
{
    public interface IAuthService
    {
        Task<ApplicationUser?> CreateUserAsync(RegisterModel model);
        Task<ApplicationUser?> GetUserByEmailAsync(string email);
        Task<ApplicationUser?> ValidateUserAsync(LoginModel model);
    }
}
