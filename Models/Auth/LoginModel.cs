using System.ComponentModel.DataAnnotations;

namespace BLOGAURA.Models.Auth
{
    public class LoginModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
