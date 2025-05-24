using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrustTrade.DAL.Abstract;
using TrustTrade.Models;
using TrustTrade.Services.Web.Interfaces;

namespace TrustTrade.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly IReportService _reportService;
        private readonly IUserService _userService;
        private readonly ILogger<ReportController> _logger;
        private readonly IPostRepository _postRepository;

        public ReportController(
            IReportService reportService,
            IUserService userService,
            ILogger<ReportController> logger,
            IPostRepository postRepository)
        {
            _reportService = reportService;
            _userService = userService;
            _logger = logger;
            _postRepository = postRepository;
        }

        [HttpPost]
        public async Task<IActionResult> ReportPost(int postId, string category, string description)
        {
            try
            {
                var currentUser = await _userService.GetCurrentUserAsync(User);
                if (currentUser == null)
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                // Check if user has already reported this post
                bool hasReported = await _reportService.HasUserAlreadyReportedAsync(currentUser.Id, "Post", postId);
                
                if (hasReported)
                {
                    return Json(new { 
                        success = false, 
                        message = "You have already reported this post. Multiple reports for the same content may be considered spam.",
                        warning = true 
                    });
                }

                var report = await _reportService.CreateReportAsync(
                    reporterId: currentUser.Id,
                    reportType: "Post",
                    postId: postId,
                    userId: null,
                    category: category,
                    description: description
                );

                _logger.LogInformation($"User {currentUser.Id} reported post {postId} for {category}");

                return Json(new { 
                    success = true, 
                    message = "Thank you for your report. Our team will review it shortly.",
                    reportId = report.Id 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reporting post");
                return Json(new { success = false, message = "An error occurred while submitting your report." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ReportProfile(int profileId, string category, string description)
        {
            try
            {
                var currentUser = await _userService.GetCurrentUserAsync(User);
                if (currentUser == null)
                {
                    return Json(new { success = false, message = "Unauthorized" });
                }

                // Prevent self-reporting
                if (currentUser.Id == profileId)
                {
                    return Json(new { success = false, message = "You cannot report your own profile." });
                }

                // Check if user has already reported this profile
                bool hasReported = await _reportService.HasUserAlreadyReportedAsync(currentUser.Id, "Profile", profileId);
                
                if (hasReported)
                {
                    return Json(new { 
                        success = false, 
                        message = "You have already reported this profile. Multiple reports for the same content may be considered spam.",
                        warning = true 
                    });
                }

                var report = await _reportService.CreateReportAsync(
                    reporterId: currentUser.Id,
                    reportType: "Profile",
                    postId: null,
                    userId: profileId,
                    category: category,
                    description: description
                );

                _logger.LogInformation($"User {currentUser.Id} reported profile {profileId} for {category}");

                return Json(new { 
                    success = true, 
                    message = "Thank you for your report. Our team will review it shortly.",
                    reportId = report.Id 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reporting profile");
                return Json(new { success = false, message = "An error occurred while submitting your report." });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetReportInfo(string reportType, int entityId)
        {
            try
            {
                var currentUser = await _userService.GetCurrentUserAsync(User);
                if (currentUser == null)
                {
                    return Json(new { success = false });
                }

                // Get reporter info
                var reporterInfo = new
                {
                    reporterName = currentUser.Username,
                    reporterEmail = currentUser.Email
                };

                // Get reportee info based on type
                object reporteeInfo = null;
                if (reportType == "Post")
                {
                    var post = await _postRepository.FindByIdAsync(entityId);
                    if (post != null)
                    {
                        reporteeInfo = new
                        {
                            postTitle = post.Title,
                            postAuthor = post.User?.Username,
                            postId = post.Id
                        };
                    }
                }
                else if (reportType == "Profile")
                {
                    var user = await _userService.GetUserByIdAsync(entityId);
                    if (user != null)
                    {
                        reporteeInfo = new
                        {
                            profileUsername = user.Username,
                            profileId = user.Id
                        };
                    }
                }

                return Json(new
                {
                    success = true,
                    reporter = reporterInfo,
                    reportee = reporteeInfo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting report info");
                return Json(new { success = false });
            }
        }
    }
}