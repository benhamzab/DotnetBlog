using BLOGAURA.Models.Auth;
using BLOGAURA.Models.Posts;
using Microsoft.EntityFrameworkCore;

namespace BLOGAURA.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<ApplicationUser> Users { get; set; } = default!;
        public DbSet<Post> Posts { get; set; } = default!;
        public DbSet<PostImage> PostImages { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<ApplicationUser>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<Post>()
                .Property(p => p.ImagePath)
                .IsRequired(false);

            modelBuilder.Entity<Post>()
                .Property(p => p.VideoPath)
                .IsRequired(false);

            modelBuilder.Entity<PostImage>()
                .HasOne(pi => pi.Post)
                .WithMany(p => p.Images)
                .HasForeignKey(pi => pi.PostId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
