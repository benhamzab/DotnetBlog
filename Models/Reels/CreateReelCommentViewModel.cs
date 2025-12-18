using System.ComponentModel.DataAnnotations;

namespace BLOGAURA.Models.Reels
{
    public class CreateReelCommentViewModel
    {
        [Required]
        public int ReelId { get; set; }

        [Required]
        [StringLength(500)]
        public string Content { get; set; } = string.Empty;
    }
}
