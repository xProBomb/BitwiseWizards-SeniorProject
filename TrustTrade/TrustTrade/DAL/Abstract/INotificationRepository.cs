using TrustTrade.Models;

namespace TrustTrade.DAL.Abstract
{
    public interface INotificationRepository : IRepository<Notification>
    {
        Task<List<Notification>> GetUnreadNotificationsForUserAsync(int userId, int count = 10);
        Task<int> GetUnreadNotificationCountForUserAsync(int userId);
        Task<bool> MarkAsReadAsync(int notificationId);
        Task<bool> MarkAllAsReadForUserAsync(int userId);
        Task<bool> CreateNotificationAsync(int userId, string type, string message, int? entityId = null, string entityType = null, int? actorId = null);
    }
}