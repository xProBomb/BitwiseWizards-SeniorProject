using Microsoft.AspNetCore.Identity.UI.Services;
using TrustTrade.DAL.Abstract;
using TrustTrade.Models;
using TrustTrade.Services.Web.Interfaces; 

namespace TrustTrade.Services.Web.Implementations
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPostRepository _postRepository;
        private readonly INotificationService _notificationService;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ReportService> _logger;

        public ReportService(
            IReportRepository reportRepository,
            IUserRepository userRepository,
            IPostRepository postRepository,
            INotificationService notificationService,
            IEmailSender emailSender,
            ILogger<ReportService> logger)
        {
            _reportRepository = reportRepository;
            _userRepository = userRepository;
            _postRepository = postRepository;
            _notificationService = notificationService;
            _emailSender = emailSender;
            _logger = logger;
        }

        public async Task<Report> CreateReportAsync(int reporterId, string reportType, int? postId, int? userId, string category, string description)
        {
            var report = new Report
            {
                ReporterId = reporterId,
                ReportType = reportType,
                ReportedPostId = postId,
                ReportedUserId = userId,
                Category = category,
                Description = description,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            if (reportType == "Post" && postId.HasValue)
            {
                var post = await _postRepository.FindByIdAsync(postId.Value);
                if (post != null)
                {
                    report.ReportedUserId = post.UserId;
                }
                else
                {
                    _logger.LogWarning($"Report creation: Post with ID {postId.Value} not found when trying to set ReportedUserId for the report.");
                }
            }

            await _reportRepository.AddOrUpdateAsync(report);
            var detailedReport = await _reportRepository.GetReportWithDetailsAsync(report.Id);

            if (detailedReport == null)
            {
                _logger.LogError($"Failed to retrieve details for report ID {report.Id} after creation. Notifications/Email will use basic report data and may lack some details."); 
                // Pass the original 'report' object which is non-null but lacks loaded navigation properties.
                // SendReportNotificationsAsync and SendReportEmailAsync should handle this with their ?? and ?. operators.
                await SendReportNotificationsAsync(report); // Pass 'report' (basic) instead of 'detailedReport' (null)
                return report; // Return the basic report
            }
            else
            {
                // Send in-app notifications to admins and the report email to the specified address
                await SendReportNotificationsAsync(detailedReport); 
                return detailedReport; 
            }
        }

        public async Task<bool> HasUserAlreadyReportedAsync(int userId, string reportType, int entityId)
        {
            return await _reportRepository.HasUserReportedEntityAsync(userId, reportType, entityId);
        }

        public async Task SendReportNotificationsAsync(Report report) // report here is detailedReport (or basic report if detailed failed)
        {
            // Fetch admins for in-app notifications
            var admins = await _userRepository.GetAllAdminsAsync();
            if (!admins.Any())
            {
                _logger.LogWarning("No admin users found to send in-app report notifications to.");
            }

            string notificationMessage;
            // If 'report' is the basic version, report.Reporter might be null.
            string reporterName = report.Reporter?.Username ?? "A user"; 
            // If 'report' is the basic version, report.Category will still be available as it was set initially.
            string reportCategory = report.Category ?? "Unspecified";

            if (report.ReportType == "Post" && report.ReportedPost != null)
            {
                // If 'report' is the basic version, report.ReportedPost will be null. This block will be skipped.
                string postTitle = !string.IsNullOrWhiteSpace(report.ReportedPost.Title) ? report.ReportedPost.Title : "Untitled Post";
                string postOwnerName = report.ReportedPost.User?.Username ?? "an unknown user";
                notificationMessage = $"{reporterName} reported the post \"{postTitle}\" (owned by {postOwnerName}). Category: {reportCategory}.";
            }
            else if (report.ReportType == "Profile" && report.ReportedUser != null)
            {
                // If 'report' is the basic version, report.ReportedUser might be null (unless ReportedUserId was set and it's loaded, unlikely for basic).
                string reportedUserName = report.ReportedUser.Username ?? "an unknown user";
                notificationMessage = $"{reporterName} reported the profile of {reportedUserName}. Category: {reportCategory}.";
            }
            else
            {
                // This fallback will likely be hit if the basic 'report' object is used and navigation properties are null.
                string targetDescription = "an entity";
                if (report.ReportedPostId.HasValue) targetDescription = $"post ID {report.ReportedPostId.Value}";
                else if (report.ReportedUserId.HasValue) targetDescription = $"user ID {report.ReportedUserId.Value}";
                else if (!string.IsNullOrEmpty(report.ReportType)) targetDescription = $"a {report.ReportType.ToLower()}";
                
                notificationMessage = $"{reporterName} submitted a {reportCategory} report regarding {targetDescription}.";
                _logger.LogWarning($"Generating fallback report notification message for Report ID {report.Id} due to missing details. Reporter: {reporterName}, ReportType: {report.ReportType}, Category: {reportCategory}");
            }

            // Send in-app notifications to all admins
            foreach (var admin in admins)
            {
                await _notificationService.CreateNotificationAsync(
                    admin.Id,
                    "Report",
                    notificationMessage,
                    report.Id,
                    "Report",
                    report.ReporterId
                );
            }
            
            // Send email notification to the specified address
            await SendReportEmailAsync(report);
        }

        private async Task SendReportEmailAsync(Report report)
        {
            string recipientEmail = "trusttrade.auth@gmail.com"; 

            string reportedContentDetails = "";
            // If 'report' is the basic version, report.ReportedPost will be null.
            if (report.ReportType == "Post" && report.ReportedPost != null)
            {
                reportedContentDetails = $"Post Title: \"{(!string.IsNullOrWhiteSpace(report.ReportedPost.Title) ? report.ReportedPost.Title : "Untitled Post")}\"<br/>Post Author: {report.ReportedPost.User?.Username ?? "N/A"} (ID: {report.ReportedPost.UserId})<br/>Post ID: {report.ReportedPost.Id}";
            }
            // If 'report' is the basic version, report.ReportedUser will be null (unless ReportedUserId was set and it's loaded, unlikely for basic).
            else if (report.ReportType == "Profile" && report.ReportedUser != null)
            {
                reportedContentDetails = $"User Profile: {report.ReportedUser.Username ?? "N/A"}<br/>User ID: {report.ReportedUser.Id}";
            }
            else
            {
                // Fallback for basic 'report' object or if details are otherwise unavailable
                if (report.ReportType == "Post" && report.ReportedPostId.HasValue)
                {
                    reportedContentDetails = $"Post ID: {report.ReportedPostId.Value} (Full details like title/author require admin panel review if not loaded)";
                }
                else if (report.ReportType == "Profile" && report.ReportedUserId.HasValue)
                {
                    reportedContentDetails = $"User ID: {report.ReportedUserId.Value} (Full details like username require admin panel review if not loaded)";
                }
                else
                {
                    reportedContentDetails = "Details not fully available, please check admin panel for Report ID.";
                }
            }
            
            // If 'report' is basic, report.Reporter might be null.
            // report.Category and report.Id will be available from the initial object.
            string emailBody = $@"
                <html>
                <body>
                    <h2>New Content Report Submitted on TrustTrade</h2>
                    <p>A new report has been submitted and requires your attention:</p>
                    <ul>
                        <li><strong>Report ID:</strong> {report.Id}</li>
                        <li><strong>Report Type:</strong> {report.ReportType}</li>
                        <li><strong>Category:</strong> {report.Category ?? "Unspecified"}</li>
                        <li><strong>Reporter:</strong> {report.Reporter?.Username ?? "N/A"} (Email: {report.Reporter?.Email ?? "N/A"}, ID: {report.ReporterId})</li>
                        <li><strong>Date Submitted:</strong> {report.CreatedAt:yyyy-MM-dd HH:mm:ss UTC}</li>
                    </ul>
                    <h3>Reported Content Details:</h3>
                    <p>{reportedContentDetails}</p>
                    <h3>Description Provided by Reporter:</h3>
                    <p>{System.Net.WebUtility.HtmlEncode(report.Description)}</p>
                    <hr/>
                    <p>Please log in to the TrustTrade admin panel (if applicable) or review this report based on your internal procedures.</p>
                    <p><em>This is an automated notification.</em></p>
                </body>
                </html>";

            string subject = $"[TrustTrade] New {report.Category ?? "Unspecified"} Report ({report.ReportType}) - ID: {report.Id}";

            try
            {
                await _emailSender.SendEmailAsync(recipientEmail, subject, emailBody);
                _logger.LogInformation($"Report email sent to {recipientEmail} for report ID {report.Id}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send report email to {recipientEmail} for report ID {report.Id}.");
            }
        }
    }
}