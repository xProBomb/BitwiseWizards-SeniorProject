using Microsoft.EntityFrameworkCore;
using TrustTrade.DAL.Abstract;
using TrustTrade.Models;

namespace TrustTrade.DAL.Concrete
{
    public class NotificationRepository : Repository<Notification>, INotificationRepository
    {
        private readonly TrustTradeDbContext _context;
        
        public NotificationRepository(TrustTradeDbContext context) : base(context)
        {
            _context = context;
        }
        
        public async Task<List<Notification>> GetUnreadNotificationsForUserAsync(int userId, int count = 10)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .Take(count)
                .Include(n => n.Actor)
                .ToListAsync();
        }
        
        public async Task<int> GetUnreadNotificationCountForUserAsync(int userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }
        
        public async Task<bool> MarkAsReadAsync(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification == null)
                return false;
            
            notification.IsRead = true;
            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task<bool> MarkAllAsReadForUserAsync(int userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();
                
            if (!notifications.Any())
                return true;
                
            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }
            
            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task<bool> CreateNotificationAsync(int userId, string type, string message, int? entityId = null, string entityType = null, int? actorId = null)
        {
            // Check for similar recent notifications to avoid duplicates
            if (entityId.HasValue && actorId.HasValue)
            {
                // Look for similar notifications within the last hour
                var recentSimilar = await _context.Notifications
                    .Where(n => n.UserId == userId 
                                && n.Type == type 
                                && n.EntityId == entityId
                                && n.EntityType == entityType
                                && n.ActorId == actorId
                                && n.CreatedAt > DateTime.UtcNow.AddHours(-1))
                    .FirstOrDefaultAsync();
            
                if (recentSimilar != null)
                {
                    // Update existing notification instead of creating a new one
                    recentSimilar.Message = message;
                    recentSimilar.CreatedAt = DateTime.UtcNow;
                    recentSimilar.IsRead = false; // Mark as unread again
            
                    await _context.SaveChangesAsync();
                    return true;
                }
            }
    
            // If no similar notification found, create a new one
            var notification = new Notification
            {
                UserId = userId,
                Type = type,
                Message = message,
                EntityId = entityId,
                EntityType = entityType,
                ActorId = actorId,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };
    
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<List<Notification>> GetAllNotificationsForUserAsync(int userId, int page = 1, int pageSize = 20)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsArchived)
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Include(n => n.Actor)
                .ToListAsync();
        }
        public async Task<bool> ArchiveNotificationAsync(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification == null)
                return false;
    
            notification.IsArchived = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ArchiveAllNotificationsForUserAsync(int userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsArchived)
                .ToListAsync();
        
            if (!notifications.Any())
                return true;
        
            foreach (var notification in notifications)
            {
                notification.IsArchived = true;
            }
    
            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task<int> GetTotalNotificationsCountForUserAsync(int userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsArchived);
        }
    }
}