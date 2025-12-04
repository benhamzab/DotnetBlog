using BLOGAURA.Models.Posts;

namespace BLOGAURA.Services.Posts
{
    public interface IPostService
    {
        Task<Post> CreatePostAsync(int userId, CreatePostModel model);
        Task<List<Post>> GetLatestPostsAsync(int count);
        Task<List<Post>> GetUserPostsAsync(int userId);
    }
}
