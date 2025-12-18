using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace BLOGAURA.Models.Auth
{
    public class EditProfileSettingsViewModel
    {
        [StringLength(100)]
        public string? DisplayName { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(256)]
        public string? UserName { get; set; }

        [DataType(DataType.Upload)]
        public IFormFile? NewProfilePhoto { get; set; }

        public string? CurrentPhotoUrl { get; set; }

        public int? ProfileUserId { get; set; }
    }
}
