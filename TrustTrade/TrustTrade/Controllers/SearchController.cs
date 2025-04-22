using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrustTrade.Services.Web.Interfaces;
using TrustTrade.ViewModels;
using TrustTrade.DAL.Abstract;
using TrustTrade.Models;
using TrustTrade.DAL.Concrete;
using TrustTrade.Data;


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

        [HttpGet("/Search")]
        public IActionResult Search()
        {
            return View();
        }

        [HttpGet("Search/Posts")]
        public async Task<IActionResult> SearchPosts(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return PartialView("_PostSearchResultsPartial", new List<PostPreviewVM>());

            var searchTerms = search.Split(' ').ToList();
            var postPreviews = await _postService.SearchPostsAsync(searchTerms);

            return PartialView("_PostSearchResultsPartial", postPreviews);
        }

        [HttpGet("Search/SearchUsers")]
        public async Task<IActionResult> SearchUsers(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return PartialView("_UserSearchResultsPartial", new List<User>());

            var users = await _searchuserRepository.SearchUsersAsync(search);
            return PartialView("_UserSearchResultsPartial", users);
        }

    }
}
