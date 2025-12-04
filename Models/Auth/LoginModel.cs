using System.ComponentModel.DataAnnotations;

namespace BLOGAURA.Models.Auth
{
    public class LoginModel
    {
        [EmailAddress]
        public string? Email { get; set; }

        public string? Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
