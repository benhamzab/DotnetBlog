using BLOGAURA.Models.Auth;
using System.ComponentModel.DataAnnotations;

namespace BLOGAURA.Models.Posts
{
    public class PostLike
    {
        public int Id { get; set; }

        public int PostId { get; set; }
        public Post Post { get; set; } = null!;

        public int UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
