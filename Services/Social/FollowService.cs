using System;
using System.Linq;
using System.Threading.Tasks;
using BLOGAURA.Data;
using BLOGAURA.Models.Social;
using Microsoft.EntityFrameworkCore;

namespace BLOGAURA.Services.Social
{
    public class FollowService : IFollowService
    {
        private readonly ApplicationDbContext _context;

        public FollowService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> IsFollowingAsync(int currentUserId, int targetUserId)
        {
            return await _context.UserFollows
                .AnyAsync(uf => uf.FollowerId == currentUserId && uf.FollowedId == targetUserId);
        }

        public async Task FollowAsync(int currentUserId, int targetUserId)
        {
            // Prevent self-follow
            if (currentUserId == targetUserId)
                return;

            // Check if already following
            var existingFollow = await _context.UserFollows
                .FirstOrDefaultAsync(uf => uf.FollowerId == currentUserId && uf.FollowedId == targetUserId);

            if (existingFollow == null)
            {
                var userFollow = new UserFollow
                {
                    FollowerId = currentUserId,
                    FollowedId = targetUserId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.UserFollows.Add(userFollow);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UnfollowAsync(int currentUserId, int targetUserId)
        {
            var userFollow = await _context.UserFollows
                .FirstOrDefaultAsync(uf => uf.FollowerId == currentUserId && uf.FollowedId == targetUserId);

            if (userFollow != null)
            {
                _context.UserFollows.Remove(userFollow);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetFollowersCountAsync(int userId)
        {
            return await _context.UserFollows
                .CountAsync(uf => uf.FollowedId == userId);
        }

        public async Task<int> GetFollowingCountAsync(int userId)
        {
            return await _context.UserFollows
                .CountAsync(uf => uf.FollowerId == userId);
        }
    }
}
