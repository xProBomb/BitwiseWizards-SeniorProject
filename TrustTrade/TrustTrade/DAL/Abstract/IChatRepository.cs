using TrustTrade.Models;

namespace TrustTrade.DAL.Abstract
{
    public interface IChatRepository
    {
        // Conversation methods
        Task<Conversation> GetConversationAsync(int conversationId);
        Task<Conversation> GetConversationAsync(int user1Id, int user2Id);
        Task<List<Conversation>> GetUserConversationsAsync(int userId, int skip = 0, int take = 20);
        Task<int> GetUnreadMessagesCountAsync(int userId);
        Task<Conversation> CreateConversationAsync(int user1Id, int user2Id);
        Task<bool> ArchiveConversationAsync(int conversationId, int userId);
        
        // Message methods
        Task<Message> GetMessageAsync(int messageId);
        Task<List<Message>> GetConversationMessagesAsync(int conversationId, int skip = 0, int take = 50);
        Task<Message> SendMessageAsync(int conversationId, int senderId, int recipientId, string content);
        Task<bool> MarkMessagesAsReadAsync(int conversationId, int userId);
        Task<bool> DeleteMessageAsync(int messageId, int userId);
    }
}