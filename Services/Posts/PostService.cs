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
                throw new ArgumentException("Title is required", nameof(model));
            }

            if (string.IsNullOrWhiteSpace(model.Content))
            {
                throw new ArgumentException("Content is required", nameof(model));
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
            try
            {
                if (model.ImageFiles != null && model.ImageFiles.Count > 0)
                {
                    if (string.IsNullOrEmpty(_environment.WebRootPath))
                    {
                        throw new InvalidOperationException("WebRootPath is not configured. Cannot save images.");
                    }

                    var imagesRoot = Path.Combine(_environment.WebRootPath, "uploads", "posts");
                    Directory.CreateDirectory(imagesRoot);

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
                            ImagePath = $"/uploads/posts/{fileName}"
                        };
                        _context.PostImages.Add(image);
                    }

                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't fail the entire post creation
                Console.WriteLine($"Error saving images: {ex.Message}");
                // The post is already saved, so we can continue
                // In production, you might want to handle this differently
            }

            return post;
        }

        public Task<List<Post>> GetLatestPostsAsync(int count)
        {
            return _context.Posts
                .Include(p => p.User)
                .Include(p => p.Images)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .OrderByDescending(p => p.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Post>> GetUserPostsAsync(int userId)
        {
            return await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Images)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Post?> GetPostByIdAsync(int postId)
        {
            return await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Images)
                .Include(p => p.Likes)
                .Include(p => p.Comments).ThenInclude(c => c.User)
                .FirstOrDefaultAsync(p => p.Id == postId);
        }

        public async Task UpdatePostAsync(Post post, EditPostViewModel model, IWebHostEnvironment environment)
        {
            // Update basic fields
            post.Title = model.Title.Trim();
            post.Content = model.Content.Trim();

            // Handle image removal
            if (model.RemoveImage && !string.IsNullOrEmpty(post.ImagePath))
            {
                DeleteImageFile(post.ImagePath, environment);
                post.ImagePath = null;
            }

            // Handle new image upload
            if (model.NewImage != null && model.NewImage.Length > 0)
            {
                // Delete old image if exists
                if (!string.IsNullOrEmpty(post.ImagePath))
                {
                    DeleteImageFile(post.ImagePath, environment);
                }

                // Save new image
                var uploadsFolder = Path.Combine(environment.WebRootPath, "uploads", "posts");
                Directory.CreateDirectory(uploadsFolder);

                var extension = Path.GetExtension(model.NewImage.FileName).ToLowerInvariant();
                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.NewImage.CopyToAsync(stream);
                }

                post.ImagePath = $"/uploads/posts/{fileName}";
            }

            _context.Posts.Update(post);
            await _context.SaveChangesAsync();
        }

        public async Task DeletePostAsync(Post post, IWebHostEnvironment environment)
        {
            // Delete associated image file if exists
            if (!string.IsNullOrEmpty(post.ImagePath))
            {
                DeleteImageFile(post.ImagePath, environment);
            }

            // Delete all associated PostImages files
            foreach (var postImage in post.Images)
            {
                if (!string.IsNullOrEmpty(postImage.ImagePath))
                {
                    DeleteImageFile(postImage.ImagePath, environment);
                }
            }

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
        }

        private static void DeleteImageFile(string imagePath, IWebHostEnvironment environment)
        {
            try
            {
                var fullPath = Path.Combine(environment.WebRootPath, imagePath.TrimStart('/'));
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
            catch (Exception ex)
            {
                // Log but don't fail the operation
                Console.WriteLine($"Could not delete image file {imagePath}: {ex.Message}");
            }
        }
    }
}
