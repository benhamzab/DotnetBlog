using System.Threading.Tasks;

namespace BLOGAURA.Services.Social
{
    public interface IFollowService
    {
        Task<bool> IsFollowingAsync(int currentUserId, int targetUserId);
        Task FollowAsync(int currentUserId, int targetUserId);
        Task UnfollowAsync(int currentUserId, int targetUserId);
        Task<int> GetFollowersCountAsync(int userId);
        Task<int> GetFollowingCountAsync(int userId);
    }
}
