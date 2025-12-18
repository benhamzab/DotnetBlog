using BLOGAURA.Models.Auth;
using System.ComponentModel.DataAnnotations;

namespace BLOGAURA.Models.Posts
{
    public class Post
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1500)]
        public string Content { get; set; } = string.Empty;

        public string? ImagePath { get; set; }

        public string? VideoPath { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ApplicationUser? User { get; set; }

        public ICollection<PostImage> Images { get; set; } = new List<PostImage>();

        public ICollection<PostLike> Likes { get; set; } = new List<PostLike>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Repost> Reposts { get; set; } = new List<Repost>();
    }
}
