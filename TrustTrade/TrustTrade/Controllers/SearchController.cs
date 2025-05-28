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
using TrustTrade.Models.ExtensionMethods;


namespace TrustTrade.Controllers
{
    public class SearchController : Controller
    {
        private readonly ISearchUserRepository _searchuserRepository;
        private readonly IUserService _userService;
        private readonly IPostService _postService;

        public SearchController(
            ISearchUserRepository searchuserRepository,
            IUserService userService,
            IPostService postService)
        {
            _searchuserRepository = searchuserRepository;
            _userService = userService;
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

            User? currentUser = await _userService.GetCurrentUserAsync(User);

            var searchTerms = search.Split(' ').ToList();
            var posts = await _postService.SearchPostsAsync(searchTerms, currentUser?.Id);

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

            return PartialView("_PostSearchResultsPartial", posts.ToPreviewViewModels());
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
