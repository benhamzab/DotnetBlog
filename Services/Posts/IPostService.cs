using BLOGAURA.Models.Posts;

namespace BLOGAURA.Services.Posts
{
    public interface IPostService
    {
        Task<Post> CreatePostAsync(int userId, CreatePostModel model);
        Task<List<Post>> GetLatestPostsAsync(int count);
        Task<List<Post>> GetUserPostsAsync(int userId);
        Task<Post?> GetPostByIdAsync(int postId);
        Task UpdatePostAsync(Post post, EditPostViewModel model, IWebHostEnvironment environment);
        Task DeletePostAsync(Post post, IWebHostEnvironment environment);
    }
}
