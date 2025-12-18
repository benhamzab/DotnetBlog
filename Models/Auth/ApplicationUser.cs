using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace BLOGAURA.Models.Auth
{
    public class ApplicationUser : IdentityUser<int>
    {
        // Custom properties beyond IdentityUser
        
        // Profile picture URL (legacy name, kept for compatibility)
        public string? ProfilePictureUrl { get; set; }
        
        // Photo URL (new Identity-style property, alias to ProfilePictureUrl)
        public string? PhotoUrl 
        { 
            get => ProfilePictureUrl; 
            set => ProfilePictureUrl = value; 
        }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // User bio/about section
        [StringLength(500)]
        public string? About { get; set; }
        
        // Student profile information
        [StringLength(200)]
        public string? University { get; set; }
        
        [StringLength(100)]
        public string? Grade { get; set; }
        
        [StringLength(100)]
        public string? City { get; set; }
        
        public int? Age { get; set; }
        
        [StringLength(500)]
        public string? Hobbies { get; set; }
        
        [StringLength(300)]
        public string? FavoriteEventTypes { get; set; }
        
        [StringLength(300)]
        public string? LearningNow { get; set; }
        
        // Social links
        [StringLength(200)]
        public string? GithubUrl { get; set; }
        
        [StringLength(200)]
        public string? LinkedinUrl { get; set; }
        
        [StringLength(200)]
        public string? PortfolioUrl { get; set; }

        [StringLength(100)]
        public string? DisplayName { get; set; }
        
        // Follow relationships
        public ICollection<Social.UserFollow> Following { get; set; } = new List<Social.UserFollow>();
        public ICollection<Social.UserFollow> Followers { get; set; } = new List<Social.UserFollow>();
        
        // Note: The following properties are now inherited from IdentityUser<int>:
        // - Id (int)
        // - UserName (string) - replaces our old Username
        // - Email (string)
        // - PasswordHash (string)
        // - EmailConfirmed, PhoneNumber, TwoFactorEnabled, etc.
    }
}
