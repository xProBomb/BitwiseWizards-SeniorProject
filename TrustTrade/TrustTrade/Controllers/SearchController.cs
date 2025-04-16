using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TrustTrade.Data;
using TrustTrade.Models;
using System.Collections.Generic;
using TrustTrade.Services.Web.Interfaces;
using TrustTrade.ViewModels;

namespace TrustTrade.Controllers
{
    public class SearchController : Controller
    {
        private readonly ISearchUserRepository _searchuserRepository;
        private readonly IPostService _postService;

        public SearchController(
            ISearchUserRepository searchuserRepository, 
            IPostService postService)
        {
            _searchuserRepository = searchuserRepository;
            _postService = postService;
        }

        // Renders the main Search page
        [HttpGet("/Search")]
        public IActionResult Search()
        {
            return View();
        }

        [HttpGet("Search/Posts")]
        public async Task<IActionResult> SearchPosts(string search, string? categoryFilter = null, int pageNumber = 1, string sortOrder = "DateDesc")
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return RedirectToAction("Search");
            }

            List<string> searchTerms = search.Split(' ').ToList();
            List<PostPreviewVM> postPreviews = await _postService.SearchPostsAsync(searchTerms, categoryFilter, pageNumber, sortOrder);

            PostFiltersPartialVM postFiltersVM = await _postService.BuildPostFiltersAsync(categoryFilter, sortOrder, search);
            PaginationPartialVM paginationVM = await _postService.BuildSearchPaginationAsync(search, searchTerms, categoryFilter, pageNumber);

            var vm = new SearchPostResultsVM
            {
                Posts = postPreviews,
                Pagination = paginationVM,
                PostFilters = postFiltersVM,
                SearchQuery = search,
            };

            return View("SearchPostResults", vm);
        }

        // This action is called asynchronously to search for users in real time
        [HttpGet]
        public async Task<IActionResult> SearchUsers(string searchTerm)
        {
            // some validation, prob want to add more?
            if (searchTerm?.Length > 50)
            {
                return BadRequest("Search term is too long.");
            }

            var users = await _searchuserRepository.SearchUsersAsync(searchTerm);

            
            return PartialView("_UserSearchResultsPartial", users);
        }

    }
}
