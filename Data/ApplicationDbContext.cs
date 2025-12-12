using BLOGAURA.Models.Auth;
using BLOGAURA.Models.Posts;
using BLOGAURA.Models.Social;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BLOGAURA.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Custom DbSets (Identity manages Users automatically as AspNetUsers)
        public DbSet<Post> Posts { get; set; } = default!;
        public DbSet<PostImage> PostImages { get; set; } = default!;
        public DbSet<UserFollow> UserFollows { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // IMPORTANT: Call base.OnModelCreating to configure Identity tables
            base.OnModelCreating(modelBuilder);

            // Custom ApplicationUser configurations
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                // Email must be unique
                entity.HasIndex(u => u.Email).IsUnique();
                
                // UserName must be unique (Identity already does this, but being explicit)
                entity.HasIndex(u => u.UserName).IsUnique();
            });

            // UserFollow configurations (self-referencing many-to-many)
            modelBuilder.Entity<UserFollow>(entity =>
            {
                // Composite primary key
                entity.HasKey(uf => new { uf.FollowerId, uf.FollowedId });

                // Follower relationship
                entity.HasOne(uf => uf.Follower)
                    .WithMany(u => u.Following)
                    .HasForeignKey(uf => uf.FollowerId)
                    .OnDelete(DeleteBehavior.NoAction);

                // Followed relationship
                entity.HasOne(uf => uf.Followed)
                    .WithMany(u => u.Followers)
                    .HasForeignKey(uf => uf.FollowedId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // Post configurations
            modelBuilder.Entity<Post>(entity =>
            {
                entity.Property(p => p.ImagePath).IsRequired(false);
                entity.Property(p => p.VideoPath).IsRequired(false);
            });

            // PostImage configurations
            modelBuilder.Entity<PostImage>(entity =>
            {
                entity.HasOne(pi => pi.Post)
                    .WithMany(p => p.Images)
                    .HasForeignKey(pi => pi.PostId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}

