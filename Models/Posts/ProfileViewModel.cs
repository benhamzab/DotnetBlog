using BLOGAURA.Models.Auth;
using System.Collections.Generic;

namespace BLOGAURA.Models.Posts
{
    public class ProfileViewModel
    {
        public ApplicationUser User { get; set; } = null!;
        public List<Post> Posts { get; set; } = new();
        public bool IsFollowing { get; set; }
        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }
    }
}
