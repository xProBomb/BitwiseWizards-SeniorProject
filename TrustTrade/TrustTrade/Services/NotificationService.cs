using TrustTrade.DAL.Abstract;
using TrustTrade.Models;
using Microsoft.EntityFrameworkCore;

namespace TrustTrade.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPostRepository _postRepository;
        private readonly TrustTradeDbContext _context;
        private readonly ILogger<NotificationService> _logger;

        private readonly Dictionary<string, DateTime> _lastNotificationTimes = new();
        private readonly TimeSpan _minimumInterval = TimeSpan.FromMinutes(5);

        public NotificationService(
            INotificationRepository notificationRepository,
            IUserRepository userRepository,
            IPostRepository postRepository,
            TrustTradeDbContext context,
            ILogger<NotificationService> logger)
        {
            _notificationRepository = notificationRepository;
            _userRepository = userRepository;
            _postRepository = postRepository;
            _context = context;
            _logger = logger;
        }

        public async Task<List<Notification>> GetUnreadNotificationsAsync(int userId, int count = 10)
        {
            return await _notificationRepository.GetUnreadNotificationsForUserAsync(userId, count);
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _notificationRepository.GetUnreadNotificationCountForUserAsync(userId);
        }

        public async Task<bool> MarkAsReadAsync(int notificationId, int currentUserId)
        {
            // Verify the notification belongs to the current user
            var notification = await _notificationRepository.FindByIdAsync(notificationId);
            if (notification == null || notification.UserId != currentUserId)
                return false;

            return await _notificationRepository.MarkAsReadAsync(notificationId);
        }

        public async Task<bool> MarkAllAsReadAsync(int userId)
        {
            return await _notificationRepository.MarkAllAsReadForUserAsync(userId);
        }

        // New method to get user notification settings
        private async Task<NotificationSettings> GetUserSettingsAsync(int userId)
        {
            var settings = await _context.NotificationSettings
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (settings == null)
            {
                // Create default settings if none exist
                settings = new NotificationSettings
                {
                    UserId = userId,
                    EnableFollowNotifications = true,
                    EnableLikeNotifications = true,
                    EnableCommentNotifications = true,
                    EnableMentionNotifications = true,
                    EnableMessageNotifications = true
                };
                _context.NotificationSettings.Add(settings);
                await _context.SaveChangesAsync();
            }

            return settings;
        }

        public async Task CreateFollowNotificationAsync(int followerId, int followingUserId)
        {
            try
            {
                // Check if the user wants follow notifications
                var settings = await GetUserSettingsAsync(followingUserId);
                if (!settings.EnableFollowNotifications)
                    return;

                var follower = await _userRepository.FindByIdAsync(followerId);
                if (follower == null)
                {
                    _logger.LogWarning(
                        $"Failed to create follow notification: Follower with ID {followerId} not found");
                    return;
                }

                string message = $"{follower.Username} started following you";

                await _notificationRepository.CreateNotificationAsync(
                    followingUserId,
                    "Follow",
                    message,
                    followerId,
                    "User",
                    followerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating follow notification for user {followingUserId}");
            }
        }

        public async Task CreateLikeNotificationAsync(int actorId, int postId, int postOwnerId)
        {
            try
            {
                // Don't notify users about their own actions
                if (actorId == postOwnerId)
                    return;

                // Check if the user wants like notifications
                var settings = await GetUserSettingsAsync(postOwnerId);
                if (!settings.EnableLikeNotifications)
                    return;

                // Rate limiting check
                string notificationKey = $"like:{actorId}:{postId}";
                if (_lastNotificationTimes.TryGetValue(notificationKey, out DateTime lastTime))
                {
                    if (DateTime.UtcNow - lastTime < _minimumInterval)
                    {
                        _logger.LogWarning($"Rate limited notification from user {actorId} liking post {postId}");
                        return; // Skip creating notification due to rate limit
                    }
                }

                // Update the last notification time
                _lastNotificationTimes[notificationKey] = DateTime.UtcNow;

                var actor = await _userRepository.FindByIdAsync(actorId);
                var post = await _postRepository.FindByIdAsync(postId);

                if (actor == null || post == null)
                {
                    _logger.LogWarning(
                        $"Failed to create like notification: Actor or Post not found. ActorId: {actorId}, PostId: {postId}");
                    return;
                }

                string message = $"{actor.Username} liked your post \"{TruncateTitle(post.Title)}\"";

                await _notificationRepository.CreateNotificationAsync(
                    postOwnerId,
                    "Like",
                    message,
                    postId,
                    "Post",
                    actorId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating like notification for post {postId}");
            }
        }

        public async Task CreateCommentNotificationAsync(int actorId, int postId, int postOwnerId, int commentId)
        {
            try
            {
                // Don't notify users about their own actions
                if (actorId == postOwnerId)
                    return;

                // Check if the user wants comment notifications
                var settings = await GetUserSettingsAsync(postOwnerId);
                if (!settings.EnableCommentNotifications)
                    return;

                var actor = await _userRepository.FindByIdAsync(actorId);
                var post = await _postRepository.FindByIdAsync(postId);

                if (actor == null || post == null)
                {
                    _logger.LogWarning(
                        $"Failed to create comment notification: Actor or Post not found. ActorId: {actorId}, PostId: {postId}");
                    return;
                }

                string message = $"{actor.Username} commented on your post \"{TruncateTitle(post.Title)}\"";

                await _notificationRepository.CreateNotificationAsync(
                    postOwnerId,
                    "Comment",
                    message,
                    commentId,
                    "Comment",
                    actorId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating comment notification for post {postId}");
            }
        }

        public async Task CreateMentionNotificationAsync(int actorId, int entityId, string entityType,
            int mentionedUserId)
        {
            try
            {
                // Don't notify users about their own actions
                if (actorId == mentionedUserId)
                    return;

                // Check if the user wants mention notifications
                var settings = await GetUserSettingsAsync(mentionedUserId);
                if (!settings.EnableMentionNotifications)
                    return;

                var actor = await _userRepository.FindByIdAsync(actorId);
                if (actor == null)
                {
                    _logger.LogWarning($"Failed to create mention notification: Actor not found. ActorId: {actorId}");
                    return;
                }

                string entityDesc = "";

                // Get context based on entity type
                if (entityType == "Post")
                {
                    var post = await _postRepository.FindByIdAsync(entityId);
                    if (post != null)
                    {
                        entityDesc = $" in post \"{TruncateTitle(post.Title)}\"";
                    }
                }
                else if (entityType == "Comment")
                {
                    entityDesc = " in a comment";
                }

                string message = $"{actor.Username} mentioned you{entityDesc}";

                await _notificationRepository.CreateNotificationAsync(
                    mentionedUserId,
                    "Mention",
                    message,
                    entityId,
                    entityType,
                    actorId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating mention notification for user {mentionedUserId}");
            }
        }

        public async Task<List<Notification>> GetAllNotificationsAsync(int userId, int page = 1, int pageSize = 20)
        {
            return await _notificationRepository.GetAllNotificationsForUserAsync(userId, page, pageSize);
        }

        public async Task<int> GetTotalNotificationsCountAsync(int userId)
        {
            return await _notificationRepository.GetTotalNotificationsCountForUserAsync(userId);
        }

        public async Task<bool> ArchiveNotificationAsync(int notificationId, int currentUserId)
        {
            // Verify the notification belongs to the current user
            var notification = await _notificationRepository.FindByIdAsync(notificationId);
            if (notification == null || notification.UserId != currentUserId)
                return false;

            return await _notificationRepository.ArchiveNotificationAsync(notificationId);
        }

        public async Task<bool> ArchiveAllNotificationsAsync(int userId)
        {
            return await _notificationRepository.ArchiveAllNotificationsForUserAsync(userId);
        }

        // Helper to truncate long post titles for notification messages
        private string TruncateTitle(string title, int maxLength = 50)
        {
            if (string.IsNullOrEmpty(title))
                return "";

            if (title.Length <= maxLength)
                return title;

            return title.Substring(0, maxLength - 3) + "...";
        }
    }
}