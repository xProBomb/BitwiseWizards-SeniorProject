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
        
        [AllowAnonymous]
        public IActionResult Landing()
        {
            return View();
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index(string? categoryFilter = null, string sortOrder = "DateDesc")
        {
            int pageNumber = 1; // Load the first page and let the JS handle the rest

            User? currentUser = await _userService.GetCurrentUserAsync(User, includeRelated: true);

            // Retrieve posts for the general feed
            (List<Post> posts, int totalPosts) = await _postService.GetPagedPostsAsync(categoryFilter, pageNumber, sortOrder, currentUser?.Id);
            PostFiltersPartialVM postFiltersVM = await _postService.BuildPostFiltersAsync(categoryFilter, sortOrder);
            PaginationPartialVM paginationVM = await _postService.BuildPaginationAsync(categoryFilter, pageNumber, totalPosts, currentUser?.Id);

            if (currentUser != null)
            {
                // Get IDs of users the current user follows
                var followingUserIds = currentUser.FollowerFollowingUsers
                    .Select(f => f.FollowerUserId)
                    .ToHashSet();

                // Show public posts, own private posts, and private posts from followed users
                posts = posts.Where(p =>
                    p.IsPublic ||
                    (!p.IsPublic && p.UserId == currentUser.Id) ||
                    (!p.IsPublic && followingUserIds.Contains(p.UserId))
                ).ToList();
            }
            else
            {
                // If not logged in, filter out private posts
                posts = posts.Where(p => p.IsPublic).ToList();
            }

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
        public async Task<IActionResult> Following(string? categoryFilter = null, string sortOrder = "DateDesc")
        {
            int pageNumber = 1; // Load the first page and let the JS handle the rest

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
