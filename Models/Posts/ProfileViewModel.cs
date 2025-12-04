using BLOGAURA.Models.Auth;
using System.Collections.Generic;

namespace BLOGAURA.Models.Posts
{
    public class ProfileViewModel
    {
        public ApplicationUser User { get; set; } = null!;
        public List<Post> Posts { get; set; } = new();
    }
}
