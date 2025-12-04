using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace BLOGAURA.Models.Posts
{
    public class CreatePostModel
    {
        [Required]
        [StringLength(200, ErrorMessage = "Le titre ne doit pas dépasser 200 caractères.")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1500, ErrorMessage = "Le contenu ne doit pas dépasser 1500 caractères.")]
        public string Content { get; set; } = string.Empty;

        // Unified media file (image ou vidéo) - conservé pour compatibilité
        [Display(Name = "Média")]
        [DataType(DataType.Upload)]
        public IFormFile? MediaFile { get; set; }

        // Multiple images (optional)
        [Display(Name = "Images")]
        [DataType(DataType.Upload)]
        public List<IFormFile>? ImageFiles { get; set; }

        // Legacy fields kept for compatibility if needed
        [Display(Name = "Image")]
        public IFormFile? ImageFile { get; set; }

        [Display(Name = "Vidéo (MP4)")]
        [DataType(DataType.Upload)]
        public IFormFile? VideoFile { get; set; }
    }
}
