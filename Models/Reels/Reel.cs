using BLOGAURA.Models.Auth;
using BLOGAURA.Models.Posts;
using System.ComponentModel.DataAnnotations;

namespace BLOGAURA.Models.Reels
{
    public class Reel
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;

        public int? EventPostId { get; set; }
        public BLOGAURA.Models.Posts.Post? EventPost { get; set; }

        [StringLength(300)]
        public string Caption { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string VideoPath { get; set; } = string.Empty;

        [StringLength(500)]
        public string? ThumbnailPath { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [StringLength(100)]
        public string? Category { get; set; }

        public ICollection<ReelLike> Likes { get; set; } = new List<ReelLike>();
        public ICollection<ReelComment> Comments { get; set; } = new List<ReelComment>();
    }

    public class ReelLike
    {
        public int Id { get; set; }
        public int ReelId { get; set; }
        public Reel Reel { get; set; } = null!;

        public int UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class ReelComment
    {
        public int Id { get; set; }
        public int ReelId { get; set; }
        public Reel Reel { get; set; } = null!;

        public int UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;

        [Required]
        [StringLength(500)]
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
