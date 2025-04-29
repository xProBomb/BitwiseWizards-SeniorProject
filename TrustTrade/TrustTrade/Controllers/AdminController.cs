using Microsoft.AspNetCore.Mvc;
using TrustTrade.Models;
using TrustTrade.DAL.Abstract;
using TrustTrade.ViewModels;
using Microsoft.AspNetCore.Authorization;
using TrustTrade.Helpers;
using TrustTrade.Services.Web.Interfaces;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace TrustTrade.Controllers
{
    public class AdminController : Controller
    {
        private readonly ILogger<PostsController> _logger;
        private readonly IPostRepository _postRepository;
        private readonly IUserService _userService;
        private readonly IAdminService _adminService;
        private readonly IEmailSender _emailSender;

        public AdminController(
            ILogger<PostsController> logger,
            IPostRepository postRepository,
            IUserService userService,
            IAdminService adminService,
            IEmailSender emailSender)
        {
            _logger = logger;
            _postRepository = postRepository;
            _userService = userService;
            _adminService = adminService;
            _emailSender = emailSender;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeletePost(int id)
        {
            var post = await _postRepository.FindByIdAsync(id);
            if (post == null) return NotFound();

            var currentUser = await _userService.GetCurrentUserAsync(User);
            if (currentUser == null || !currentUser.IsAdmin) return Unauthorized();

            var postOwner = post.User;
            await _postRepository.DeleteAsync(post);

            if (postOwner != null)
            {
                var subject = "Post Deleted - TrustTrade";
                var body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <h2>Post Deletion Notice</h2>
                    <p>Hello {postOwner.Username},</p>
                    <p>Your post titled <strong>{post.Title}</strong> has been removed by an administrator due to a violation of our platform policies.</p>
                    <p>If you believe this was a mistake, you can contact support to appeal.</p>
                    <p style='color: gray;'>- TrustTrade Support Team</p>
                </body>
                </html>";

                await _emailSender.SendEmailAsync(postOwner.Email, subject, body);
            }

            return RedirectToAction("Index", "Home");
        }
        public class UserActionDto
        {
            public int userId { get; set; }
        }
        [HttpPost("/Admin/SuspendUser")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> SuspendUser([FromBody] UserActionDto dto)
        {
            var userId = dto.userId;
            var currentUser = await _adminService.GetCurrentUserAsync(User);
            if (currentUser == null || !currentUser.IsAdmin) return Unauthorized();

            var user = await _adminService.FindUserByIdAsync(userId);
            if (user == null) return NotFound();

            await _adminService.SuspendUserAsync(userId);
            var email = user.Email;
            var subject = "Account Suspension Notice - TrustTrade";
            var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <h2>Account Suspended</h2>
                <p>Hello {user.Username},</p>
                <p>Your account has been suspended due to a violation of our platform policies.</p>
                <p>If you believe this suspension is in error, you may <a href='https://trusttrade.support/contact'>contact support</a> to appeal.</p>
                <p style='color: gray;'>- TrustTrade Support Team</p>
            </body>
            </html>";
            _logger.LogInformation($"Suspending user {user.Username} with ID {userId}.");
            _logger.LogInformation($"Sending email to {email} with {subject} and {body}.");
            await _emailSender.SendEmailAsync(email, subject, body);

            return Ok();
        }
    

        [HttpGet]
        public async Task<IActionResult> ManageUsers()
        {
            var currentUser = await _adminService.GetCurrentUserAsync(User);
            if (currentUser == null || !currentUser.IsAdmin) return Unauthorized();

            var users = await _adminService.GetAllTrustTradeUsersAsync();
            return View(users);
        }

        [HttpPost("/Admin/UnsuspendUser")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> UnsuspendUser([FromBody] UserActionDto dto)
        {
            var userId = dto.userId;
            var currentUser = await _adminService.GetCurrentUserAsync(User);
            if (currentUser == null || !currentUser.IsAdmin) return Unauthorized();

            var user = await _adminService.FindUserByIdAsync(userId);
            if (user == null) return NotFound();

            await _adminService.UnsuspendUserAsync(userId);

            var subject = "Account Reactivation Notice - TrustTrade";
            var body = $@"
            <html>
            <body style='font-family: Arial, sans-serif;'>
                <h2>Account Reactivated</h2>
                <p>Hello {user.Username},</p>
                <p>Your TrustTrade account has been reactivated. You can now log in and resume activity on the platform.</p>
                <p>We appreciate your cooperation and understanding.</p>
                <p style='color: gray;'>- TrustTrade Support Team</p>
            </body>
            </html>";

            await _emailSender.SendEmailAsync(user.Email, subject, body);

            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> SearchUsers(string searchTerm)
        {
            var currentUser = await _adminService.GetCurrentUserAsync(User);
            if (currentUser == null || !currentUser.IsAdmin) return Unauthorized();

            var users = await _adminService.SearchTrustTradeUsersAsync(searchTerm);
            return PartialView("_UserListPartial", users);
        }
    }
}
