using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TrustTrade.Models;
using TrustTrade.ViewModels;
using Microsoft.AspNetCore.Authorization;
using TrustTrade.Services.Web.Interfaces;

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

        public async Task<IActionResult> Index(string? categoryFilter = null, int pageNumber = 1, string sortOrder = "DateDesc")
        {
            // Retrieve posts
            List<PostPreviewVM> postPreviews = await _postService.GetPostPreviewsAsync(categoryFilter, pageNumber, sortOrder);
    
            // Build filters and pagination
            PostFiltersPartialVM postFiltersVM = await _postService.BuildPostFiltersAsync(categoryFilter, sortOrder);
            PaginationPartialVM paginationVM = await _postService.BuildPaginationAsync(categoryFilter, pageNumber);

            var vm = new IndexVM
            {
                Posts = postPreviews,
                Pagination = paginationVM,
                PostFilters = postFiltersVM,
            };

            return View(vm);
        }

        [Authorize]
        public async Task<IActionResult> Following(string? categoryFilter = null, int pageNumber = 1, string sortOrder = "DateDesc")
        {
            User? user = await _userService.GetCurrentUserAsync(User);
            if (user == null) return Unauthorized();            

            int currentUserId = user.Id;

            // Retrieve posts
            List<PostPreviewVM> postPreviews = await _postService.GetFollowingPostPreviewsAsync(currentUserId, categoryFilter, pageNumber, sortOrder);

            // Build filters and pagination
            PostFiltersPartialVM postFiltersVM = await _postService.BuildPostFiltersAsync(categoryFilter, sortOrder);
            PaginationPartialVM paginationVM = await _postService.BuildFollowingPaginationAsync(currentUserId, categoryFilter, pageNumber);

            var vm = new IndexVM
            {
                Posts = postPreviews,
                Pagination = paginationVM,
                PostFilters = postFiltersVM,
            };

            return View(vm);
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
