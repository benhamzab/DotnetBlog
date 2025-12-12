using System;
using BLOGAURA.Models.Auth;

namespace BLOGAURA.Models.Social
{
    public class UserFollow
    {
        public int FollowerId { get; set; }
        public ApplicationUser Follower { get; set; } = null!;

        public int FollowedId { get; set; }
        public ApplicationUser Followed { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
