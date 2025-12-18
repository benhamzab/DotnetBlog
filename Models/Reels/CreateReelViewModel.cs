using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace BLOGAURA.Models.Reels
{
    public class CreateReelViewModel
    {
        [Required]
        [Display(Name = "Vidéo (MP4)")]
        [DataType(DataType.Upload)]
        public IFormFile? VideoFile { get; set; }

        [StringLength(300)]
        [Display(Name = "Caption")]
        public string? Caption { get; set; }

        [Display(Name = "Post d'événement lié (optionnel)")]
        public int? EventPostId { get; set; }

        [StringLength(100)]
        [Display(Name = "Catégorie d'événement (tech, sport, culture)")]
        public string? Category { get; set; }
    }
}
