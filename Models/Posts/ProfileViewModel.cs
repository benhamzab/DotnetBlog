using BLOGAURA.Models.Auth;
using System.Collections.Generic;

namespace BLOGAURA.Models.Posts
{
    public class ProfileViewModel
    {
        public ApplicationUser User { get; set; } = null!;

        public string ProfileUserId { get; set; } = string.Empty;
        public string CurrentUserId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
        public string? CoverImageUrl { get; set; }
        public string? Bio { get; set; }

        public int PostsCount { get; set; }
        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }

        public List<Post> Posts { get; set; } = new();
        public List<Post> Replies { get; set; } = new();
        public List<Post> Reposts { get; set; } = new();
        public List<Post> Likes { get; set; } = new();
        public List<BLOGAURA.Models.Reels.Reel> Reels { get; set; } = new();

        public bool IsFollowing { get; set; }
    }
}
