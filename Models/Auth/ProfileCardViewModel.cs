using System;

namespace BLOGAURA.Models.Auth
{
    public class ProfileCardViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public string? Email { get; set; }
        public string RoleLabel { get; set; } = "Membre BlogAura";
        public int PostsCount { get; set; }
        public int FollowingCount { get; set; }
        public int FollowersCount { get; set; }
        public string? PhotoUrl { get; set; }
    }
}
