using System.Collections.Generic;
using System.Threading.Tasks;
using TrustTrade.Models;
using TrustTrade.Models.ViewModels;

namespace TrustTrade.Services.Web.Interfaces
{
    public interface IChatService
    {
        // Conversation methods
        Task<Conversation> GetConversationAsync(int conversationId, int currentUserId);
        Task<Conversation> GetOrCreateConversationAsync(int user1Id, int user2Id);
        Task<List<ChatConversationVM>> GetUserConversationsAsync(int userId, int page = 1, int pageSize = 20);
        Task<int> GetUnreadMessagesCountAsync(int userId);
        Task<bool> ArchiveConversationAsync(int conversationId, int userId);
        
        // Message methods
        Task<List<ChatMessageVM>> GetConversationMessagesAsync(int conversationId, int currentUserId, int page = 1, int pageSize = 50);
        Task<ChatMessageVM> SendMessageAsync(int conversationId, int senderId, int recipientId, string content);
        Task<bool> MarkConversationAsReadAsync(int conversationId, int userId);
        Task<bool> DeleteMessageAsync(int messageId, int userId);
        
        // Other utility methods
        Task<int> GetOtherUserIdAsync(int conversationId, int currentUserId);
        Task<bool> CanAccessConversationAsync(int conversationId, int userId);
    }
}