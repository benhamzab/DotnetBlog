using Microsoft.EntityFrameworkCore;
using BLOGAURA.Models;

namespace BLOGAURA.Data
{
    public class BlogContext : DbContext
    {
        public BlogContext(DbContextOptions<BlogContext> options) : base(options)

        {
        }

        public DbSet<Post> Posts { get; set; } = default!;
        public DbSet<Category> Categories { get; set; } = default!;
        public DbSet<Comment> Comments { get; set; } = default!;
        public DbSet<User> Users { get; set; } = default!;
        public DbSet<Tag> Tags { get; set; } = default!;
        public DbSet<PostTag> PostTags { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Composite key for PostTag
            modelBuilder.Entity<PostTag>()
                .HasKey(pt => new { pt.PostId, pt.TagId });

            // Post → PostTags
            modelBuilder.Entity<PostTag>()
                .HasOne(pt => pt.Post)
                .WithMany(p => p.PostTags)
                .HasForeignKey(pt => pt.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            // Tag → PostTags
            modelBuilder.Entity<PostTag>()
                .HasOne(pt => pt.Tag)
                .WithMany(t => t.PostTags)
                .HasForeignKey(pt => pt.TagId)
                .OnDelete(DeleteBehavior.Cascade);

            // Post → Category
            modelBuilder.Entity<Post>()
                .HasOne(p => p.Category)
                .WithMany()
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Post → User
            modelBuilder.Entity<Post>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }

    }
}
