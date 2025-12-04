using System.ComponentModel.DataAnnotations;

namespace BLOGAURA.Models.Posts
{
    public class PostImage
    {
        public int Id { get; set; }

        [Required]
        public int PostId { get; set; }

        [Required]
        [StringLength(500)]
        public string ImagePath { get; set; } = string.Empty;

        public Post? Post { get; set; }
    }
}
