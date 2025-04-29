using Microsoft.AspNetCore.Mvc;
using TrustTrade.Models;
using TrustTrade.DAL.Abstract;
using TrustTrade.ViewModels;
using Microsoft.AspNetCore.Authorization;
using TrustTrade.Helpers;
using TrustTrade.Services.Web.Interfaces;

namespace TrustTrade.Controllers
{
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly IPostRepository _postRepository;
        private readonly IUserService _userService;
        private readonly IAdminService _adminService;

        public AdminController(
            ILogger<AdminController> logger,
            IPostRepository postRepository,
            IUserService userService,
            IAdminService adminService)
        {
            _logger = logger;
            _postRepository = postRepository;
            _userService = userService;
            _adminService = adminService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeletePost(int id)
        {
            var post = await _postRepository.FindByIdAsync(id);
            if (post == null)
                return NotFound();

            var user = await _userService.GetCurrentUserAsync(User);
            if (user == null || !user.IsAdmin)
                return Unauthorized();

            await _postRepository.DeleteAsync(post);

            _logger.LogInformation("Admin {Username} deleted post {PostId}", user.Username, id);

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> ManageUsers()
        {
            var currentUser = await _adminService.GetCurrentUserAsync(User);

            _logger.LogWarning("Admin check: Username={0}, IsAdmin={1}, IdentityId={2}",
                currentUser?.Username ?? "null",
                currentUser?.IsAdmin.ToString() ?? "null",
                currentUser?.IdentityId ?? "null");

            if (currentUser == null || !currentUser.IsAdmin)
                return Unauthorized();

            var users = await _adminService.GetAllTrustTradeUsersAsync();
            return View(users);
        }

        public class UserIdRequest
        {
            public int userId { get; set; }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuspendUser([FromBody] UserIdRequest request)
        {
            var currentUser = await _adminService.GetCurrentUserAsync(User);
            if (currentUser == null || !currentUser.IsAdmin)
                return Unauthorized();

            await _adminService.SuspendUserAsync(request.userId);

            _logger.LogInformation("Admin {Username} suspended user {UserId}", currentUser.Username, request.userId);
            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnsuspendUser([FromBody] UserIdRequest request)
        {
            var currentUser = await _adminService.GetCurrentUserAsync(User);
            if (currentUser == null || !currentUser.IsAdmin)
                return Unauthorized();

            await _adminService.UnsuspendUserAsync(request.userId);

            _logger.LogInformation("Admin {Username} unsuspended user {UserId}", currentUser.Username, request.userId);
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> SearchUsers(string searchTerm)
        {
            var currentUser = await _adminService.GetCurrentUserAsync(User);
            if (currentUser == null || !currentUser.IsAdmin)
                return Unauthorized();

            var users = await _adminService.SearchTrustTradeUsersAsync(searchTerm);
            return PartialView("_UserListPartial", users);
        }
    }
}
