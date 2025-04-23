using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public NotificationsController(
            INotificationService notificationService,
            IUserService userService,
            ILogger<NotificationsController> logger,
            INotificationRepository notificationRepository)
        {
            _notificationService = notificationService;
            _userService = userService;
            _logger = logger;
            _notificationRepository = notificationRepository;
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

                // Mark all as read when viewing the full page
                await _notificationService.MarkAllAsReadAsync(currentUser.Id);

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

                // Here we would retrieve notification settings from the database
                // For now, let's create a simple model with default settings
                var viewModel = new NotificationSettingsViewModel
                {
                    EnableFollowNotifications = true,
                    EnableLikeNotifications = true,
                    EnableCommentNotifications = true,
                    EnableMentionNotifications = true,
                    EnableMessageNotifications = true
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving notification settings");
                return View("Error", new ErrorViewModel { ErrorMessage = "Failed to load notification settings." });
            }
        }

        /// <summary>
        /// Updates notification settings
        /// </summary>
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

                // Here we would save the notification settings to the database
                // For now, just return success
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
                var MaxCount = 25;
                var totalPages = (int)Math.Ceiling(MaxCount / (double)PageSize);

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

        [HttpPost("Archive")]
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

                // If standard form submission
                if (success)
                {
                    TempData["SuccessMessage"] = "Notification archived successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to archive notification.";
                }

                return RedirectToAction("History");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error archiving notification");
                return Json(new { success = false });
            }
        }

        [HttpPost("ArchiveAll")]
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

                // If standard form submission
                if (success)
                {
                    TempData["SuccessMessage"] = "All notifications archived.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to archive notifications.";
                }

                return RedirectToAction("History");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error archiving all notifications");
                return Json(new { success = false });
            }
        }
    }
}