using BLOGAURA.Models.Auth;
using System.Collections.Generic;

namespace BLOGAURA.Models.Social
{
    public class FollowUserItem
    {
        public int UserId { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
    }

    public class FollowListViewModel
    {
        public string ProfileUserId { get; set; } = string.Empty;
        public string ProfileDisplayName { get; set; } = string.Empty;
        public bool IsFollowers { get; set; }
        public List<FollowUserItem> Users { get; set; } = new();
    }
}
