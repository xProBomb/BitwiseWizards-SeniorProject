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
        private readonly ILogger<PostsController> _logger;
        private readonly IPostRepository _postRepository;
        private readonly IUserService _userService;

        private readonly IAdminService _adminService;

        public AdminController(ILogger<PostsController> logger, IPostRepository postRepository, IUserService userService, IAdminService adminService)
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
            Post? post = await _postRepository.FindByIdAsync(id);
            if (post == null) return NotFound();

            User? user = await _userService.GetCurrentUserAsync(User);
            if (user == null) return Unauthorized();

            // Ensure the user is an admin
            if (user.IsAdmin != true) return Unauthorized();

            await _postRepository.DeleteAsync(post);

            return RedirectToAction("Index", "Home");
        }
        public async Task<IActionResult> ManageUsers()
        {
            var currentUser = await _adminService.GetCurrentUserAsync(User);
            if (currentUser == null || currentUser.IsAdmin != true)
                return Unauthorized();

            var users = await _adminService.GetAllTrustTradeUsersAsync(); // No Identity users
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> SuspendUser(int userId)
        {
            var currentUser = await _adminService.GetCurrentUserAsync(User);
            if (currentUser == null || currentUser.IsAdmin != true)
                return Unauthorized();

            await _adminService.SuspendUserAsync(userId);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> UnsuspendUser(int userId)
        {
            var currentUser = await _adminService.GetCurrentUserAsync(User);
            if (currentUser == null || currentUser.IsAdmin != true)
                return Unauthorized();

            await _adminService.UnsuspendUserAsync(userId);
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> SearchUsers(string searchTerm)
        {
            var currentUser = await _adminService.GetCurrentUserAsync(User);
            if (currentUser == null || currentUser.IsAdmin != true)
                return Unauthorized();

            var users = await _adminService.SearchTrustTradeUsersAsync(searchTerm);
            return PartialView("_UserListPartial", users);
        }
    }
}