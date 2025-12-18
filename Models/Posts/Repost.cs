using BLOGAURA.Models.Auth;

namespace BLOGAURA.Models.Posts
{
    public class Repost
    {
        public int Id { get; set; }

        public int OriginalPostId { get; set; }
        public Post OriginalPost { get; set; } = null!;

        public int UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
