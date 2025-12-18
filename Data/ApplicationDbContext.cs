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
        public DbSet<BLOGAURA.Models.Posts.PostLike> PostLikes { get; set; } = default!;
        public DbSet<BLOGAURA.Models.Posts.Comment> Comments { get; set; } = default!;
        public DbSet<BLOGAURA.Models.Posts.Repost> Reposts { get; set; } = default!;
        public DbSet<BLOGAURA.Models.Content.ContentCalendarItem> ContentCalendar { get; set; } = default!;

        public DbSet<BLOGAURA.Models.Reels.Reel> Reels { get; set; } = default!;
        public DbSet<BLOGAURA.Models.Reels.ReelLike> ReelLikes { get; set; } = default!;
        public DbSet<BLOGAURA.Models.Reels.ReelComment> ReelComments { get; set; } = default!;

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

            modelBuilder.Entity<BLOGAURA.Models.Posts.PostLike>(entity =>
            {
                entity.HasIndex(l => new { l.PostId, l.UserId }).IsUnique();
                entity.HasOne(l => l.Post)
                    .WithMany(p => p.Likes)
                    .HasForeignKey(l => l.PostId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(l => l.User)
                    .WithMany()
                    .HasForeignKey(l => l.UserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<BLOGAURA.Models.Posts.Comment>(entity =>
            {
                entity.Property(c => c.Content).HasMaxLength(1000);
                entity.HasOne(c => c.Post)
                    .WithMany(p => p.Comments)
                    .HasForeignKey(c => c.PostId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(c => c.User)
                    .WithMany()
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<BLOGAURA.Models.Posts.Repost>(entity =>
            {
                entity.HasIndex(r => new { r.OriginalPostId, r.UserId }).IsUnique();
                entity.HasOne(r => r.OriginalPost)
                    .WithMany(p => p.Reposts)
                    .HasForeignKey(r => r.OriginalPostId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(r => r.User)
                    .WithMany()
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<BLOGAURA.Models.Content.ContentCalendarItem>(entity =>
            {
                entity.Property(c => c.Title).HasMaxLength(200).IsRequired();
                entity.Property(c => c.ContentType).HasMaxLength(100).IsRequired();
                entity.Property(c => c.TargetAudience).HasMaxLength(200);
                entity.Property(c => c.Status).HasMaxLength(50).IsRequired();
                entity.Property(c => c.Notes).HasMaxLength(1000);

                entity.HasIndex(c => c.PlannedPublishDate);
                entity.HasIndex(c => c.Status);
                entity.HasIndex(c => c.ContentType);
                entity.HasIndex(c => c.EditorUserId);

                entity.HasOne<BLOGAURA.Models.Posts.Post>()
                    .WithMany()
                    .HasForeignKey(c => c.PostId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Reels configurations
            modelBuilder.Entity<BLOGAURA.Models.Reels.Reel>(entity =>
            {
                entity.HasOne(r => r.User)
                    .WithMany()
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(r => r.EventPost)
                    .WithMany()
                    .HasForeignKey(r => r.EventPostId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<BLOGAURA.Models.Reels.ReelLike>(entity =>
            {
                entity.HasOne(rl => rl.Reel)
                    .WithMany(r => r.Likes)
                    .HasForeignKey(rl => rl.ReelId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(rl => rl.User)
                    .WithMany()
                    .HasForeignKey(rl => rl.UserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            modelBuilder.Entity<BLOGAURA.Models.Reels.ReelComment>(entity =>
            {
                entity.HasOne(rc => rc.Reel)
                    .WithMany(r => r.Comments)
                    .HasForeignKey(rc => rc.ReelId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(rc => rc.User)
                    .WithMany()
                    .HasForeignKey(rc => rc.UserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // Ignore legacy BlogContext join entity in this context to prevent confusion
            modelBuilder.Ignore<BLOGAURA.Models.PostTag>();
        }
    }
}
