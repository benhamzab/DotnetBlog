using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace BLOGAURA.Models.Posts
{
    public class EditPostViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Le titre est requis")]
        [StringLength(200, ErrorMessage = "Le titre ne doit pas dépasser 200 caractères")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Le contenu est requis")]
        [StringLength(1500, ErrorMessage = "Le contenu ne doit pas dépasser 1500 caractères")]
        public string Content { get; set; } = string.Empty;

        // Existing image path (for display)
        public string? ExistingImagePath { get; set; }

        // New image upload (optional)
        [Display(Name = "Nouvelle image")]
        [DataType(DataType.Upload)]
        public IFormFile? NewImage { get; set; }

        // Flag to remove existing image
        public bool RemoveImage { get; set; }
    }
}
