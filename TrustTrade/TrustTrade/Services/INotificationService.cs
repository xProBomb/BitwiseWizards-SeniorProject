using TrustTrade.Models;

namespace TrustTrade.Services
{
    public interface INotificationService
    {
        Task<List<Notification>> GetUnreadNotificationsAsync(int userId, int count = 10);
        Task<int> GetUnreadCountAsync(int userId);
        Task<bool> MarkAsReadAsync(int notificationId, int currentUserId);
        Task<bool> MarkAllAsReadAsync(int userId);
        Task CreateFollowNotificationAsync(int followerId, int followingUserId);
        Task CreateLikeNotificationAsync(int actorId, int postId, int postOwnerId);
        Task CreateCommentNotificationAsync(int actorId, int postId, int postOwnerId, int commentId);
        Task CreateMentionNotificationAsync(int actorId, int entityId, string entityType, int mentionedUserId);
    }
}