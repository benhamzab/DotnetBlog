using BLOGAURA.Data;
using BLOGAURA.Models.Posts;
using Microsoft.EntityFrameworkCore;

namespace BLOGAURA.Services.Posts
{
    public class PostService : IPostService
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public PostService(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<Post> CreatePostAsync(int userId, CreatePostModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Title))
            {
                throw new ArgumentException("Title is required", nameof(model.Title));
            }

            if (string.IsNullOrWhiteSpace(model.Content))
            {
                throw new ArgumentException("Content is required", nameof(model.Content));
            }

            var post = new Post
            {
                UserId = userId,
                Title = model.Title.Trim(),
                Content = model.Content.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            // Save multiple images under wwwroot/images
            var imagesRoot = Path.Combine(_environment.WebRootPath, "images");
            Directory.CreateDirectory(imagesRoot);

            if (model.ImageFiles != null && model.ImageFiles.Any())
            {
                foreach (var file in model.ImageFiles)
                {
                    if (file == null || file.Length == 0) continue;

                    var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                    var fileName = $"{Guid.NewGuid()}{extension}";
                    var filePath = Path.Combine(imagesRoot, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var image = new PostImage
                    {
                        PostId = post.Id,
                        ImagePath = $"/images/{fileName}"
                    };
                    _context.PostImages.Add(image);
                }

                await _context.SaveChangesAsync();
            }

            return post;
        }

        public Task<List<Post>> GetLatestPostsAsync(int count)
        {
            return _context.Posts
                .Include(p => p.User)
                .OrderByDescending(p => p.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public Task<List<Post>> GetUserPostsAsync(int userId)
        {
            return _context.Posts
                .Include(p => p.User)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }
    }
}
