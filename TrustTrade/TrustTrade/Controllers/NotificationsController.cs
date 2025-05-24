using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrustTrade.Services;
using TrustTrade.Services.Web.Interfaces;
using TrustTrade.Models;
using TrustTrade.Models.ViewModels;
using TrustTrade.DAL.Abstract;

namespace TrustTrade.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly INotificationService _notificationService;
        private readonly INotificationRepository _notificationRepository;
        private readonly IUserService _userService;
        private readonly ILogger<NotificationsController> _logger;
        private readonly IUserRepository _userRepository;
        private readonly TrustTradeDbContext _context;
        private readonly IReportRepository _reportRepository;

        public NotificationsController(
            INotificationService notificationService,
            IUserService userService,
            ILogger<NotificationsController> logger,
            INotificationRepository notificationRepository,
            IUserRepository userRepository,
            TrustTradeDbContext context,
            IReportRepository reportRepository
            )
        {
            _notificationService = notificationService;
            _userService = userService;
            _logger = logger;
            _notificationRepository = notificationRepository;
            _userRepository = userRepository;
            _context = context;
            _reportRepository = reportRepository;
        }

        /// <summary>
        /// Displays the main notifications page with all notifications
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var currentUser = await _userService.GetCurrentUserAsync(User);
                if (currentUser == null)
                    return Unauthorized();

                var notifications = await _notificationService.GetUnreadNotificationsAsync(currentUser.Id, 50);

                // Mark all as read when viewing the full page (maybe not wanted?)
                // await _notificationService.MarkAllAsReadAsync(currentUser.Id);

                return View(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notifications");
                return View("Error", new ErrorViewModel { ErrorMessage = "Failed to load notifications." });
            }
        }

        /// <summary>
        /// Returns the count of unread notifications for the current user
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            try
            {
                var currentUser = await _userService.GetCurrentUserAsync(User);
                if (currentUser == null)
                    return Unauthorized();

                int count = await _notificationService.GetUnreadCountAsync(currentUser.Id);

                return Json(new { count });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notification count");
                return Json(new { count = 0 });
            }
        }

        /// <summary>
        /// Returns the partial view with latest notifications for the dropdown
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetNotificationsDropdown()
        {
            try
            {
                var currentUser = await _userService.GetCurrentUserAsync(User);
                if (currentUser == null)
                    return Unauthorized();

                var notifications = await _notificationService.GetUnreadNotificationsAsync(currentUser.Id, 10);

                return PartialView("_NotificationsDropdown", notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notifications for dropdown");
                return Content("Error loading notifications");
            }
        }

        /// <summary>
        /// Marks a single notification as read
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            try
            {
                var currentUser = await _userService.GetCurrentUserAsync(User);
                if (currentUser == null)
                    return Unauthorized();

                bool success = await _notificationService.MarkAsReadAsync(id, currentUser.Id);

                return Json(new { success });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification as read");
                return Json(new { success = false });
            }
        }

        /// <summary>
        /// Marks all notifications as read for the current user
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                var currentUser = await _userService.GetCurrentUserAsync(User);
                if (currentUser == null)
                    return Unauthorized();

                bool success = await _notificationService.MarkAllAsReadAsync(currentUser.Id);

                return Json(new { success });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read");
                return Json(new { success = false });
            }
        }

        /// <summary>
        /// Displays notification settings page
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            try
            {
                var currentUser = await _userService.GetCurrentUserAsync(User);
                if (currentUser == null)
                    return Unauthorized();

                // Get user's notification settings
                var settings = await _context.NotificationSettings
                    .FirstOrDefaultAsync(s => s.UserId == currentUser.Id);

                if (settings == null)
                {
                    // Create default settings if none exist
                    settings = new NotificationSettings
                    {
                        UserId = currentUser.Id,
                        EnableFollowNotifications = true,
                        EnableLikeNotifications = true,
                        EnableCommentNotifications = true,
                        EnableMentionNotifications = true,
                        EnableMessageNotifications = true
                    };
                    _context.NotificationSettings.Add(settings);
                    await _context.SaveChangesAsync();
                }

                var viewModel = new NotificationSettingsViewModel
                {
                    EnableFollowNotifications = settings.EnableFollowNotifications,
                    EnableLikeNotifications = settings.EnableLikeNotifications,
                    EnableCommentNotifications = settings.EnableCommentNotifications,
                    EnableMentionNotifications = settings.EnableMentionNotifications,
                    EnableMessageNotifications = settings.EnableMessageNotifications
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notification settings");
                return View("Error", new ErrorViewModel { ErrorMessage = "Failed to load notification settings." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Settings(NotificationSettingsViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var currentUser = await _userService.GetCurrentUserAsync(User);
                if (currentUser == null)
                    return Unauthorized();

                // Find or create settings
                var settings = await _context.NotificationSettings
                    .FirstOrDefaultAsync(s => s.UserId == currentUser.Id);

                if (settings == null)
                {
                    settings = new NotificationSettings
                    {
                        UserId = currentUser.Id
                    };
                    _context.NotificationSettings.Add(settings);
                }

                // Update settings with form values
                settings.EnableFollowNotifications = model.EnableFollowNotifications;
                settings.EnableLikeNotifications = model.EnableLikeNotifications;
                settings.EnableCommentNotifications = model.EnableCommentNotifications;
                settings.EnableMentionNotifications = model.EnableMentionNotifications;
                settings.EnableMessageNotifications = model.EnableMessageNotifications;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Notification settings updated successfully.";

                return RedirectToAction(nameof(Settings));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating notification settings");
                ModelState.AddModelError("", "An error occurred while saving your settings.");
                return View(model);
            }
        }

        /// <summary>
        /// API endpoint to test notification creation (for development purposes)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> TestNotification(string type, string message)
        {
            try
            {
                var currentUser = await _userService.GetCurrentUserAsync(User);
                if (currentUser == null)
                    return Unauthorized();

                // Create a test notification of the specified type
                await _notificationRepository.CreateNotificationAsync(
                    currentUser.Id,
                    type,
                    message,
                    null,
                    "Test",
                    currentUser.Id);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating test notification");
                return Json(new { success = false });
            }
        }

        [HttpGet("History")]
        public async Task<IActionResult> History(int page = 1)
        {
            try
            {
                var currentUser = await _userService.GetCurrentUserAsync(User);
                if (currentUser == null)
                    return Unauthorized();

                const int PageSize = 20;
                var notifications = await _notificationService.GetAllNotificationsAsync(currentUser.Id, page, PageSize);

                // Create pagination information
                var totalCount =
                    await _notificationRepository.GetTotalNotificationsCountForUserAsync(currentUser.Id);


                var totalPages = (int)Math.Ceiling(totalCount / (double)PageSize);

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;

                return View(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notification history");
                return View("Error", new ErrorViewModel { ErrorMessage = "Failed to load notification history." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ArchiveNotification(int id)
        {
            try
            {
                var currentUser = await _userService.GetCurrentUserAsync(User);
                if (currentUser == null)
                    return Unauthorized();

                bool success = await _notificationService.ArchiveNotificationAsync(id, currentUser.Id);

                // If AJAX request
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success });
                }

                // If standard form submission, return to the current page
                string returnUrl = Request.Headers["Referer"].ToString();
                if (string.IsNullOrEmpty(returnUrl))
                {
                    returnUrl = Url.Action("History");
                }

                if (success)
                {
                    TempData["SuccessMessage"] = "Notification archived successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to archive notification.";
                }

                return Redirect(returnUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error archiving notification");

                // Handle AJAX errors differently
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false });
                }

                return Json(new { success = false });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ArchiveAllNotifications()
        {
            try
            {
                var currentUser = await _userService.GetCurrentUserAsync(User);
                if (currentUser == null)
                    return Unauthorized();

                bool success = await _notificationService.ArchiveAllNotificationsAsync(currentUser.Id);

                // If AJAX request
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success });
                }

                // If standard form submission, return to the current page
                string returnUrl = Request.Headers["Referer"].ToString();
                if (string.IsNullOrEmpty(returnUrl))
                {
                    returnUrl = Url.Action("History");
                }

                if (success)
                {
                    TempData["SuccessMessage"] = "All notifications archived.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to archive notifications.";
                }

                return Redirect(returnUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error archiving all notifications");

                // Handle AJAX errors differently
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false });
                }

                return Json(new { success = false });
            }
        }

        /// <summary>
        /// Redirects to the appropriate content based on notification type
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> RedirectToContent(int id)
        {
            try
            {
                var currentUser = await _userService.GetCurrentUserAsync(User);
                if (currentUser == null)
                    return Unauthorized();

                var notification = await _notificationRepository.FindByIdAsync(id);
                if (notification == null || notification.UserId != currentUser.Id)
                    return NotFound();

                // Mark the notification as read
                await _notificationService.MarkAsReadAsync(id, currentUser.Id);

                // Redirect based on notification type and entity
                switch (notification.Type)
                {
                    case "Follow":
                        if (notification.ActorId.HasValue)
                        {
                            var actor = await _userRepository.FindByIdAsync(notification.ActorId.Value);
                            if (actor != null)
                            {
                                return RedirectToAction("UserProfile", "Profile", new { username = actor.Username });
                            }
                        }

                        break;

                    case "Like":
                        if (notification.EntityId.HasValue && notification.EntityType == "Post")
                        {
                            return RedirectToAction("Details", "Posts", new { id = notification.EntityId.Value });
                        }

                        break;

                    case "Comment":
                        if (notification.EntityId.HasValue)
                        {
                            if (notification.EntityType == "Post")
                            {
                                return RedirectToAction("Details", "Posts", new { id = notification.EntityId.Value });
                            }
                            else if (notification.EntityType == "Comment")
                            {
                                // Get the related post for this comment
                                var comment = await _context.Comments.FindAsync(notification.EntityId.Value);
                                if (comment != null)
                                {
                                    return RedirectToAction("Details", "Posts", new { id = comment.PostId });
                                }
                            }
                        }

                        break;
                    
                    case "Report":
                        if (notification.EntityId.HasValue && notification.EntityType == "Report")
                        {
                            // The EntityId of the notification is the Id of the Report
                            var report = await _reportRepository.GetReportWithDetailsAsync(notification.EntityId.Value);
                            if (report != null)
                            {
                                if (report.ReportType == "Post" && report.ReportedPostId.HasValue)
                                {
                                    _logger.LogInformation($"Redirecting to reported post ID: {report.ReportedPostId.Value} from report notification ID: {id}");
                                    return RedirectToAction("Details", "Posts", new { id = report.ReportedPostId.Value });
                                }
                                else if (report.ReportType == "Profile" && report.ReportedUserId.HasValue)
                                {
                                    var reportedUser = await _userRepository.FindByIdAsync(report.ReportedUserId.Value);
                                    if (reportedUser != null)
                                    {
                                        _logger.LogInformation($"Redirecting to reported user profile: {reportedUser.Username} from report notification ID: {id}");
                                        return RedirectToAction("UserProfile", "Profile", new { username = reportedUser.Username });
                                    }
                                    else
                                    {
                                        _logger.LogWarning($"Reported user (ID: {report.ReportedUserId.Value}) not found for report notification ID: {id}.");
                                    }
                                }
                                else
                                {
                                     _logger.LogWarning($"Report (ID: {report.Id}) has an unknown or incomplete target for redirection from notification ID: {id}. ReportType: {report.ReportType}, ReportedPostId: {report.ReportedPostId}, ReportedUserId: {report.ReportedUserId}");
                                }
                            }
                            else
                            {
                                _logger.LogWarning($"Report (supposedly ID: {notification.EntityId.Value}) not found for report notification ID: {id}.");
                            }
                        }
                        break;

                    // TODO: When we create mentioning with @
                    /*case "Mention":
                        if (notification.EntityId.HasValue)
                        {
                            if (notification.EntityType == "Post")
                            {
                                return RedirectToAction("Details", "Posts", new { id = notification.EntityId.Value });
                            }
                            else if (notification.EntityType == "Comment")
                            {
                                // Get the post ID from the comment
                                var comment = await _context.Comments.FindAsync(notification.EntityId.Value);
                                if (comment != null)
                                {
                                    return RedirectToAction("Details", "Posts", new { id = comment.PostId });
                                }
                            }
                        }

                        break;*/

                    case "Message":
                        if (notification.EntityId.HasValue && notification.EntityType == "Conversation")
                        {
                            return RedirectToAction("Conversation", "Chat", new { id = notification.EntityId.Value });
                        }
                        else if (notification.ActorId.HasValue)
                        {
                            var actor = await _userRepository.FindByIdAsync(notification.ActorId.Value);
                            if (actor != null)
                            {
                                // Start a new conversation with this user
                                return RedirectToAction("StartConversation", "Chat", new { userId = actor.Id });
                            }
                        }
                        break;
                }

                // If we couldn't determine a specific redirection, go to notifications page
                _logger.LogWarning(
                    $"Could not determine redirection for notification type: {notification.Type}, ID: {id}");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error redirecting from notification");
                return RedirectToAction("Index");
            }
        }
    }
}