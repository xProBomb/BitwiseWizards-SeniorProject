using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using TrustTrade.DAL.Abstract;
using TrustTrade.Services.Web.Interfaces;

namespace TrustTrade.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<ChatHub> _logger;
        
        public ChatHub(
            IChatService chatService,
            IUserRepository userRepository,
            ILogger<ChatHub> logger)
        {
            _chatService = chatService;
            _userRepository = userRepository;
            _logger = logger;
        }
        
        public override async Task OnConnectedAsync()
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user != null)
                {
                    // Add the user to a group with their ID to allow direct messaging
                    await Groups.AddToGroupAsync(Context.ConnectionId, user.Id.ToString());
                    _logger.LogInformation($"User {user.Username} (ID: {user.Id}) connected to chat");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnConnectedAsync");
            }
            
            await base.OnConnectedAsync();
        }
        
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user != null)
                {
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, user.Id.ToString());
                    _logger.LogInformation($"User {user.Username} (ID: {user.Id}) disconnected from chat");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnDisconnectedAsync");
            }
            
            await base.OnDisconnectedAsync(exception);
        }
        
        // Join a specific conversation group to receive messages
        public async Task JoinConversation(int conversationId)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null) return;
                
                // Verify the user has access to this conversation
                bool canAccess = await _chatService.CanAccessConversationAsync(conversationId, user.Id);
                if (!canAccess)
                {
                    _logger.LogWarning($"User {user.Id} attempted to join conversation {conversationId} without permission");
                    return;
                }
                
                // Add user to the conversation group
                await Groups.AddToGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
                _logger.LogInformation($"User {user.Id} joined conversation {conversationId}");
                
                // Mark messages as read when joining a conversation
                await _chatService.MarkConversationAsReadAsync(conversationId, user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error joining conversation {conversationId}");
            }
        }
        
        // Leave a conversation group
        public async Task LeaveConversation(int conversationId)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null) return;
                
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conversation_{conversationId}");
                _logger.LogInformation($"User {user.Id} left conversation {conversationId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error leaving conversation {conversationId}");
            }
        }
        
        // Send a message to a conversation
        public async Task SendMessage(int conversationId, string message)
        {
            try
            {
                var sender = await GetCurrentUserAsync();
                if (sender == null) return;
                
                // Verify the user has access to this conversation
                bool canAccess = await _chatService.CanAccessConversationAsync(conversationId, sender.Id);
                if (!canAccess)
                {
                    _logger.LogWarning($"User {sender.Id} attempted to send message to conversation {conversationId} without permission");
                    return;
                }
                
                // Get the other user in the conversation
                int recipientId = await _chatService.GetOtherUserIdAsync(conversationId, sender.Id);
                if (recipientId == 0)
                {
                    _logger.LogWarning($"Could not find recipient for conversation {conversationId}");
                    return;
                }
                
                // Save the message to the database
                var chatMessage = await _chatService.SendMessageAsync(conversationId, sender.Id, recipientId, message);
                
                // Send the message to all users in the conversation group
                await Clients.Group($"conversation_{conversationId}").SendAsync("ReceiveMessage", chatMessage);
                
                // Also send to the recipient's user group in case they're not currently in the conversation
                await Clients.Group(recipientId.ToString()).SendAsync("ReceiveNotification", conversationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending message to conversation {conversationId}");
            }
        }
        
        // Mark messages in a conversation as read
        public async Task MarkAsRead(int conversationId)
        {
            try
            {
                var user = await GetCurrentUserAsync();
                if (user == null) return;
                
                // Verify the user has access to this conversation
                bool canAccess = await _chatService.CanAccessConversationAsync(conversationId, user.Id);
                if (!canAccess)
                {
                    _logger.LogWarning($"User {user.Id} attempted to mark messages in conversation {conversationId} as read without permission");
                    return;
                }
                
                // Mark messages as read
                await _chatService.MarkConversationAsReadAsync(conversationId, user.Id);
                
                // Get the other user ID
                int otherUserId = await _chatService.GetOtherUserIdAsync(conversationId, user.Id);
                
                // Notify the other user that the messages have been read
                await Clients.Group($"conversation_{conversationId}").SendAsync("MessagesRead", user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error marking messages as read in conversation {conversationId}");
            }
        }
        
        // Helper method to get the current user
        private async Task<Models.User> GetCurrentUserAsync()
        {
            var identityId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(identityId))
            {
                return null;
            }
            
            return await _userRepository.FindByIdentityIdAsync(identityId);
        }
    }
}