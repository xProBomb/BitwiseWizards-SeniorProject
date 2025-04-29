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

        public AdminController(ILogger<PostsController> logger, IPostRepository postRepository, IUserService userService)
        {
            _userService = userService;
            _logger = logger;
            _postRepository = postRepository;
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
    }
}