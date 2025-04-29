using TrustTrade.DAL.Abstract;
using TrustTrade.Models;
using TrustTrade.Services.Web.Interfaces;
using TrustTrade.Models.ViewModels;

namespace TrustTrade.Services.Web.Implementations
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepository;
        private readonly IUserRepository _userRepository;
        private readonly INotificationService _notificationService;
        private readonly ILogger<ChatService> _logger;
        
        public ChatService(
            IChatRepository chatRepository,
            IUserRepository userRepository,
            INotificationService notificationService,
            ILogger<ChatService> logger)
        {
            _chatRepository = chatRepository;
            _userRepository = userRepository;
            _notificationService = notificationService;
            _logger = logger;
        }
        
        public async Task<Conversation> GetConversationAsync(int conversationId, int currentUserId)
        {
            var conversation = await _chatRepository.GetConversationAsync(conversationId);
            
            // Check if the user is part of this conversation
            if (conversation == null || (conversation.User1Id != currentUserId && conversation.User2Id != currentUserId))
            {
                return null;
            }
            
            return conversation;
        }
        
        public async Task<Conversation> GetOrCreateConversationAsync(int user1Id, int user2Id)
        {
            try
            {
                // Try to get existing conversation
                var conversation = await _chatRepository.GetConversationAsync(user1Id, user2Id);
                
                // If no conversation exists, create a new one
                if (conversation == null)
                {
                    conversation = await _chatRepository.CreateConversationAsync(user1Id, user2Id);
                }
                
                return conversation;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting or creating conversation between users {user1Id} and {user2Id}");
                throw;
            }
        }
        
        public async Task<List<ChatConversationVM>> GetUserConversationsAsync(int userId, int page = 1, int pageSize = 20)
        {
            try
            {
                int skip = (page - 1) * pageSize;
                var conversations = await _chatRepository.GetUserConversationsAsync(userId, skip, pageSize);
                
                var viewModels = new List<ChatConversationVM>();
                
                foreach (var conversation in conversations)
                {
                    // Determine the other user in the conversation
                    int otherUserId = conversation.User1Id == userId ? conversation.User2Id : conversation.User1Id;
                    var otherUser = conversation.User1Id == userId ? conversation.User2 : conversation.User1;
                    
                    // Count unread messages from the other user
                    var messages = await _chatRepository.GetConversationMessagesAsync(conversation.Id);
                    int unreadCount = messages.Count(m => m.RecipientId == userId && !m.IsRead);
                    
                    var lastMessage = messages.OrderByDescending(m => m.CreatedAt).FirstOrDefault();
                    
                    viewModels.Add(new ChatConversationVM
                    {
                        Id = conversation.Id,
                        OtherUserId = otherUserId,
                        OtherUsername = otherUser.Username,
                        OtherUserProfilePicture = otherUser.ProfilePicture,
                        LastMessage = conversation.LastMessageContent ?? "Start a conversation",
                        LastMessageTime = conversation.UpdatedAt,
                        HasUnreadMessages = unreadCount > 0,
                        UnreadCount = unreadCount
                    });
                }
                
                return viewModels;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting conversations for user {userId}");
                throw;
            }
        }
        
        public async Task<int> GetUnreadMessagesCountAsync(int userId)
        {
            try
            {
                return await _chatRepository.GetUnreadMessagesCountAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting unread message count for user {userId}");
                throw;
            }
        }
        
        public async Task<bool> ArchiveConversationAsync(int conversationId, int userId)
        {
            try
            {
                // Verify the user is part of the conversation
                var conversation = await _chatRepository.GetConversationAsync(conversationId);
                if (conversation == null || (conversation.User1Id != userId && conversation.User2Id != userId))
                {
                    return false;
                }
                
                return await _chatRepository.ArchiveConversationAsync(conversationId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error archiving conversation {conversationId} for user {userId}");
                throw;
            }
        }
        
        public async Task<List<ChatMessageVM>> GetConversationMessagesAsync(int conversationId, int currentUserId, int page = 1, int pageSize = 50)
        {
            try
            {
                // Verify the user is part of the conversation
                var conversation = await _chatRepository.GetConversationAsync(conversationId);
                if (conversation == null || (conversation.User1Id != currentUserId && conversation.User2Id != currentUserId))
                {
                    return new List<ChatMessageVM>();
                }
                
                int skip = (page - 1) * pageSize;
                var messages = await _chatRepository.GetConversationMessagesAsync(conversationId, skip, pageSize);
                
                // Mark messages as read
                await _chatRepository.MarkMessagesAsReadAsync(conversationId, currentUserId);
                
                // Convert to view models
                var viewModels = new List<ChatMessageVM>();
                foreach (var message in messages)
                {
                    var sender = await _userRepository.FindByIdAsync(message.SenderId);
                    
                    viewModels.Add(new ChatMessageVM
                    {
                        Id = message.Id,
                        ConversationId = message.ConversationId,
                        SenderId = message.SenderId,
                        SenderUsername = sender?.Username,
                        SenderProfilePicture = sender?.ProfilePicture,
                        Content = message.Content,
                        CreatedAt = message.CreatedAt,
                        IsRead = message.IsRead,
                        ReadAt = message.ReadAt,
                        IsOwnMessage = message.SenderId == currentUserId
                    });
                }
                
                return viewModels;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting messages for conversation {conversationId}");
                throw;
            }
        }
        
        public async Task<ChatMessageVM> SendMessageAsync(int conversationId, int senderId, int recipientId, string content)
        {
            try
            {
                // Verify the conversation exists and the users are part of it
                var conversation = await _chatRepository.GetConversationAsync(conversationId);
                if (conversation == null || 
                    !((conversation.User1Id == senderId && conversation.User2Id == recipientId) || 
                      (conversation.User1Id == recipientId && conversation.User2Id == senderId)))
                {
                    throw new ArgumentException("Invalid conversation");
                }
                
                // Send the message
                var message = await _chatRepository.SendMessageAsync(conversationId, senderId, recipientId, content);
                
                // Create notification for the recipient
                await _notificationService.CreateMessageNotificationAsync(senderId, recipientId, conversationId);
                
                // Convert to view model
                var sender = await _userRepository.FindByIdAsync(senderId);
                
                return new ChatMessageVM
                {
                    Id = message.Id,
                    ConversationId = message.ConversationId,
                    SenderId = message.SenderId,
                    SenderUsername = sender?.Username,
                    SenderProfilePicture = sender?.ProfilePicture,
                    Content = message.Content,
                    CreatedAt = message.CreatedAt,
                    IsRead = message.IsRead,
                    ReadAt = message.ReadAt,
                    IsOwnMessage = true // It's the sender's own message
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending message in conversation {conversationId}");
                throw;
            }
        }
        
        public async Task<bool> MarkConversationAsReadAsync(int conversationId, int userId)
        {
            try
            {
                // Verify the user is part of the conversation
                var conversation = await _chatRepository.GetConversationAsync(conversationId);
                if (conversation == null || (conversation.User1Id != userId && conversation.User2Id != userId))
                {
                    return false;
                }
                
                return await _chatRepository.MarkMessagesAsReadAsync(conversationId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error marking conversation {conversationId} as read for user {userId}");
                throw;
            }
        }
        
        public async Task<bool> DeleteMessageAsync(int messageId, int userId)
        {
            try
            {
                var message = await _chatRepository.GetMessageAsync(messageId);
                if (message == null || (message.SenderId != userId && message.RecipientId != userId))
                {
                    return false;
                }
                
                return await _chatRepository.DeleteMessageAsync(messageId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting message {messageId} for user {userId}");
                throw;
            }
        }
        
        public async Task<int> GetOtherUserIdAsync(int conversationId, int currentUserId)
        {
            var conversation = await _chatRepository.GetConversationAsync(conversationId);
            if (conversation == null)
            {
                return 0;
            }
            
            return conversation.User1Id == currentUserId ? conversation.User2Id : conversation.User1Id;
        }
        
        public async Task<bool> CanAccessConversationAsync(int conversationId, int userId)
        {
            var conversation = await _chatRepository.GetConversationAsync(conversationId);
            return conversation != null && (conversation.User1Id == userId || conversation.User2Id == userId);
        }
    }
}