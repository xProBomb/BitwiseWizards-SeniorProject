using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TrustTrade.Models;
using TrustTrade.ViewModels;
using Microsoft.AspNetCore.Authorization;
using TrustTrade.Services.Web.Interfaces;
using TrustTrade.Models.ExtensionMethods;

namespace TrustTrade.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IPostService _postService;
        private readonly IUserService _userService;

        public HomeController(
            ILogger<HomeController> logger, 
            IPostService postService,
            IUserService userService)
        {
            _logger = logger;
            _postService = postService;
            _userService = userService;
        }

        public async Task<IActionResult> Index(
            string? categoryFilter = null,
            int pageNumber = 1,
            string sortOrder = "DateDesc")
        {
            User? currentUser = await _userService.GetCurrentUserAsync(User);

            // Retrieve posts for the general feed
            (List<Post> posts, int totalPosts) = await _postService.GetPagedPostsAsync(categoryFilter, pageNumber, sortOrder, currentUser?.Id);
            PostFiltersPartialVM postFiltersVM = await _postService.BuildPostFiltersAsync(categoryFilter, sortOrder);
            PaginationPartialVM paginationVM = await _postService.BuildPaginationAsync(categoryFilter, pageNumber, totalPosts, currentUser?.Id);

            var vm = new IndexVM
            {
                Posts = posts.ToPreviewViewModels(currentUser?.Id),
                Pagination = paginationVM,
                PostFilters = postFiltersVM,
                IsFollowing = false
            };

            return View(vm);
        }

        [Authorize]
        public async Task<IActionResult> Following(
            string? categoryFilter = null,
            int pageNumber = 1,
            string sortOrder = "DateDesc")
        {                
            User? currentUser = await _userService.GetCurrentUserAsync(User);
            if (currentUser == null) return Unauthorized();

            int currentUserId = currentUser.Id;

            // Retrieve posts for the "following" feed
            (List<Post> posts, int totalPosts) = await _postService.GetFollowingPagedPostsAsync(currentUserId, categoryFilter, pageNumber, sortOrder);
            PostFiltersPartialVM postFiltersVM = await _postService.BuildPostFiltersAsync(categoryFilter, sortOrder);
            PaginationPartialVM paginationVM = await _postService.BuildPaginationAsync(categoryFilter, pageNumber, totalPosts, currentUserId);

            var vm = new IndexVM
            {
                Posts = posts.ToPreviewViewModels(currentUserId),
                Pagination = paginationVM,
                PostFilters = postFiltersVM,
                IsFollowing = true
            };

            return View("Index", vm);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [Route("/Home/Error/{code:int}")]
        public IActionResult Error(int code) 
        {
            return View(new ErrorViewModel { RequestId = "Test", ErrorMessage = $"Error Occured. Error Code {code}" });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
