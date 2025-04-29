using Microsoft.AspNetCore.Mvc;
using TrustTrade.Models;
using TrustTrade.DAL.Abstract;
using TrustTrade.ViewModels;
using Microsoft.AspNetCore.Authorization;
using TrustTrade.Helpers;
using TrustTrade.Services.Web.Interfaces;

namespace TrustTrade.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly IPostRepository _postRepository;
        private readonly IUserService _userService;

        public AdminController(ILogger<AdminController> logger, IPostRepository postRepository, IUserService userService)
        {
            _logger = logger;
            _postRepository = postRepository;
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> DeletePost(int id)
        {
            var post = await _postRepository.FindByIdAsync(id);
            if (post == null)
                return NotFound();

            var user = await _userService.GetCurrentUserAsync(User);
            if (user == null)
                return Unauthorized();
            await _postRepository.DeleteAsync(post);

            TempData["AdminMessage"] = "Post deleted successfully.";
            return RedirectToAction("Index", "Home");
        }
    }
}
