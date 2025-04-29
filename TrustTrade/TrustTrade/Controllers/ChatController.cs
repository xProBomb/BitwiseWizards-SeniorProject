using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrustTrade.Models;
using TrustTrade.Services.Web.Interfaces;
using TrustTrade.Models.ViewModels;

namespace TrustTrade.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly IChatService _chatService;
        private readonly IUserService _userService;
        private readonly ILogger<ChatController> _logger;
        
        public ChatController(
            IChatService chatService,
            IUserService userService,
            ILogger<ChatController> logger)
        {
            _chatService = chatService;
            _userService = userService;
            _logger = logger;
        }
        
        // GET: /Chat - Inbox view showing all conversations
        [HttpGet]
        public async Task<IActionResult> Index(int page = 1)
        {
            try
            {
                var currentUser = await _userService.GetCurrentUserAsync(User);
                if (currentUser == null)
                    return Unauthorized();
                
                int pageSize = 20;
                var conversations = await _chatService.GetUserConversationsAsync(currentUser.Id, page, pageSize);
                
                var model = new ChatIndexVM
                {
                    Conversations = conversations,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalConversations = conversations.Count // This is not accurate, but we'll improve it later
                };
                
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading chat inbox");
                return View("Error", new ErrorViewModel { ErrorMessage = "Failed to load conversations." });
            }
        }
        
        // GET: /Chat/Conversation/5 - View a specific conversation
        [HttpGet("Chat/Conversation/{id}")]
        public async Task<IActionResult> Conversation(int id, int page = 1)
        {
            try
            {
                var currentUser = await _userService.GetCurrentUserAsync(User);
                if (currentUser == null)
                    return Unauthorized();
                
                var conversation = await _chatService.GetConversationAsync(id, currentUser.Id);
                if (conversation == null)
                    return NotFound();
                
                int pageSize = 50;
                var messages = await _chatService.GetConversationMessagesAsync(id, currentUser.Id, page, pageSize);
                
                // Determine the other user in the conversation
                int otherUserId = conversation.User1Id == currentUser.Id ? conversation.User2Id : conversation.User1Id;
                var otherUser = conversation.User1Id == currentUser.Id ? conversation.User2 : conversation.User1;
                
                var model = new ChatDetailsVM
                {
                    Conversation = conversation,
                    Messages = messages,
                    CurrentUserId = currentUser.Id,
                    OtherUserId = otherUserId,
                    OtherUsername = otherUser.Username,
                    OtherUserProfilePicture = otherUser.ProfilePicture,
                    CurrentPage = page,
                    PageSize = pageSize,
                    TotalMessages = messages.Count // Not accurate, but we'll improve it later
                };
                
                // Mark messages as read when viewing the conversation
                await _chatService.MarkConversationAsReadAsync(id, currentUser.Id);
                
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading conversation {id}");
                return View("Error", new ErrorViewModel { ErrorMessage = "Failed to load conversation." });
            }
        }
        
        // POST: /Chat/StartConversation - Start a new conversation with a user
        [HttpPost]
        public async Task<IActionResult> StartConversation(int userId)
        {
            try
            {
                var currentUser = await _userService.GetCurrentUserAsync(User);
                if (currentUser == null)
                    return Unauthorized();
                
                // Check if the target user exists
                var targetUser = await _userService.GetUserByIdAsync(userId);
                if (targetUser == null)
                    return NotFound();
                
                // Get or create a conversation between the users
                var conversation = await _chatService.GetOrCreateConversationAsync(currentUser.Id, userId);
                
                return RedirectToAction("Conversation", new { id = conversation.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error starting conversation with user {userId}");
                return View("Error", new ErrorViewModel { ErrorMessage = "Failed to start conversation." });
            }
        }
        
        // POST: /Chat/SendMessage - Send a message in a conversation
        [HttpPost]
        public async Task<IActionResult> SendMessage(SendMessageVM model)
        {
            try
            {
                var currentUser = await _userService.GetCurrentUserAsync(User);
                if (currentUser == null)
                    return Unauthorized();
                
                // Verify the user has access to this conversation
                var conversation = await _chatService.GetConversationAsync(model.ConversationId, currentUser.Id);
                if (conversation == null)
                    return NotFound();
                
                // Get the recipient ID (the other user in the conversation)
                int recipientId = conversation.User1Id == currentUser.Id ? conversation.User2Id : conversation.User1Id;
                
                // Send the message
                await _chatService.SendMessageAsync(model.ConversationId, currentUser.Id, recipientId, model.Content);
                
                // If this is an AJAX request
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true });
                }
                
                // Otherwise, redirect back to the conversation
                return RedirectToAction("Conversation", new { id = model.ConversationId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending message in conversation {model.ConversationId}");
                
                // If this is an AJAX request
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, error = "Failed to send message." });
                }
                
                return View("Error", new ErrorViewModel { ErrorMessage = "Failed to send message." });
            }
        }
        
        // POST: /Chat/MarkAsRead - Mark all messages in a conversation as read
        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int conversationId)
        {
            try
            {
                var currentUser = await _userService.GetCurrentUserAsync(User);
                if (currentUser == null)
                    return Unauthorized();
                
                // Mark messages as read
                bool success = await _chatService.MarkConversationAsReadAsync(conversationId, currentUser.Id);
                
                return Json(new { success });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error marking messages as read in conversation {conversationId}");
                return Json(new { success = false });
            }
        }
        
        // POST: /Chat/ArchiveConversation - Archive a conversation
        [HttpPost]
        public async Task<IActionResult> ArchiveConversation(int conversationId)
        {
            try
            {
                var currentUser = await _userService.GetCurrentUserAsync(User);
                if (currentUser == null)
                    return Unauthorized();
                
                // Archive the conversation
                bool success = await _chatService.ArchiveConversationAsync(conversationId, currentUser.Id);
                
                // If this is an AJAX request
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success });
                }
                
                // Otherwise, redirect back to the inbox
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error archiving conversation {conversationId}");
                
                // If this is an AJAX request
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, error = "Failed to archive conversation." });
                }
                
                return View("Error", new ErrorViewModel { ErrorMessage = "Failed to archive conversation." });
            }
        }
        
        // GET: /Chat/GetUnreadCount - Get the count of unread messages for the current user
        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                var currentUser = await _userService.GetCurrentUserAsync(User);
                if (currentUser == null)
                    return Unauthorized();
                
                int count = await _chatService.GetUnreadMessagesCountAsync(currentUser.Id);
                
                return Json(new { count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread message count");
                return Json(new { count = 0 });
            }
        }
    }
}