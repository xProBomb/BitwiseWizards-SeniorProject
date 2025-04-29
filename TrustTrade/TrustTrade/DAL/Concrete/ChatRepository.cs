using Microsoft.EntityFrameworkCore;
using TrustTrade.DAL.Abstract;
using TrustTrade.Models;

namespace TrustTrade.DAL.Concrete
{
    public class ChatRepository : IChatRepository
    {
        private readonly TrustTradeDbContext _context;
        
        public ChatRepository(TrustTradeDbContext context)
        {
            _context = context;
        }
        
        public async Task<Conversation> GetConversationAsync(int conversationId)
        {
            return await _context.Conversations
                .Include(c => c.User1)
                .Include(c => c.User2)
                .FirstOrDefaultAsync(c => c.Id == conversationId);
        }
        
        public async Task<Conversation> GetConversationAsync(int user1Id, int user2Id)
        {
            // Find a conversation where the users are participants (in either order)
            return await _context.Conversations
                .Include(c => c.User1)
                .Include(c => c.User2)
                .FirstOrDefaultAsync(c => 
                    (c.User1Id == user1Id && c.User2Id == user2Id) || 
                    (c.User1Id == user2Id && c.User2Id == user1Id));
        }
        
        public async Task<List<Conversation>> GetUserConversationsAsync(int userId, int skip = 0, int take = 20)
        {
            // Get all conversations where the user is a participant
            var conversations = await _context.Conversations
                .Include(c => c.User1)
                .Include(c => c.User2)
                .Where(c => c.User1Id == userId || c.User2Id == userId)
                .OrderByDescending(c => c.UpdatedAt)
                .ToListAsync();
    
            // Filter out conversations where all messages are deleted/archived for this user
            var filteredConversations = new List<Conversation>();
    
            foreach (var conversation in conversations)
            {
                // Check if there are any messages that are not deleted for this user
                var hasVisibleMessages = await _context.Messages
                    .AnyAsync(m => m.ConversationId == conversation.Id && 
                                   ((m.SenderId == userId && !m.IsDeletedForSender) || 
                                    (m.RecipientId == userId && !m.IsDeletedForRecipient)));
        
                // Only include conversations with visible messages
                if (hasVisibleMessages)
                {
                    filteredConversations.Add(conversation);
                }
            }
    
            // Apply pagination after filtering
            return filteredConversations
                .Skip(skip)
                .Take(take)
                .ToList();
        }
        
        public async Task<int> GetUnreadMessagesCountAsync(int userId)
        {
            // Count all unread messages where the user is the recipient
            return await _context.Messages
                .CountAsync(m => m.RecipientId == userId && !m.IsRead);
        }
        
        public async Task<Conversation> CreateConversationAsync(int user1Id, int user2Id)
        {
            // Check if conversation already exists
            var existingConversation = await GetConversationAsync(user1Id, user2Id);
            if (existingConversation != null)
            {
                return existingConversation;
            }
            
            // Create new conversation
            var conversation = new Conversation
            {
                User1Id = user1Id,
                User2Id = user2Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                LastMessageContent = $"{_context.Users.Find(user1Id).Username} wants to start a conversation."
            };
            
            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();
            
            return conversation;
        }
        
        public async Task<bool> ArchiveConversationAsync(int conversationId, int userId)
        {
            var conversation = await _context.Conversations
                .FirstOrDefaultAsync(c => c.Id == conversationId && (c.User1Id == userId || c.User2Id == userId));
                
            if (conversation == null)
            {
                return false;
            }
            
            // For archive functionality, we'll mark all messages as deleted for this user
            var messages = await _context.Messages
                .Where(m => m.ConversationId == conversationId)
                .ToListAsync();
                
            foreach (var message in messages)
            {
                if (message.SenderId == userId)
                {
                    message.IsDeletedForSender = true;
                }
                
                if (message.RecipientId == userId)
                {
                    message.IsDeletedForRecipient = true;
                }
                
                // If both users have deleted the message, mark it as fully deleted
                if (message.IsDeletedForSender && message.IsDeletedForRecipient)
                {
                    message.IsDeleted = true;
                }
            }
            
            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task<Message> GetMessageAsync(int messageId)
        {
            return await _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Recipient)
                .FirstOrDefaultAsync(m => m.Id == messageId);
        }
        
        public async Task<List<Message>> GetConversationMessagesAsync(int conversationId, int skip = 0, int take = 50)
        {
            return await _context.Messages
                .Where(m => m.ConversationId == conversationId && !m.IsDeleted)
                .OrderBy(m => m.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }
        
        public async Task<Message> SendMessageAsync(int conversationId, int senderId, int recipientId, string content)
        {
            var conversation = await _context.Conversations.FindAsync(conversationId);
            if (conversation == null)
            {
                throw new ArgumentException("Conversation not found");
            }
            
            // Create and save the message
            var message = new Message
            {
                ConversationId = conversationId,
                SenderId = senderId,
                RecipientId = recipientId,
                Content = content,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };
            
            _context.Messages.Add(message);
            
            // Update the conversation's last message and timestamp
            conversation.LastMessageContent = content;
            conversation.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveChangesAsync();
            return message;
        }
        
        public async Task<bool> MarkMessagesAsReadAsync(int conversationId, int userId)
        {
            var messages = await _context.Messages
                .Where(m => m.ConversationId == conversationId && m.RecipientId == userId && !m.IsRead)
                .ToListAsync();
                
            if (!messages.Any())
            {
                return true;
            }
            
            DateTime now = DateTime.UtcNow;
            
            foreach (var message in messages)
            {
                message.IsRead = true;
                message.ReadAt = now;
            }
            
            await _context.SaveChangesAsync();
            return true;
        }
        
        public async Task<bool> DeleteMessageAsync(int messageId, int userId)
        {
            var message = await _context.Messages.FindAsync(messageId);
            if (message == null)
            {
                return false;
            }
            
            // Mark the message as deleted for the appropriate user
            if (message.SenderId == userId)
            {
                message.IsDeletedForSender = true;
            }
            
            if (message.RecipientId == userId)
            {
                message.IsDeletedForRecipient = true;
            }
            
            // If both users have deleted the message, mark it as fully deleted
            if (message.IsDeletedForSender && message.IsDeletedForRecipient)
            {
                message.IsDeleted = true;
            }
            
            await _context.SaveChangesAsync();
            return true;
        }
    }
}