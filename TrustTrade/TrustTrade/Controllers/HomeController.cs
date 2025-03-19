using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TrustTrade.Models;
using TrustTrade.DAL.Abstract;
using TrustTrade.ViewModels;
using TrustTrade.Helpers;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace TrustTrade.Controllers
{
    public class HomeController : Controller
    {
        private readonly TrustTradeDbContext _context;
        private readonly ILogger<HomeController> _logger;
        private readonly IPostRepository _postRepository;
        private readonly ITagRepository _tagRepository;

        public HomeController(
            TrustTradeDbContext context,
            ILogger<HomeController> logger, 
            IPostRepository postRepository, 
            ITagRepository tagRepository)
        {
            _context = context;
            _logger = logger;
            _postRepository = postRepository;
            _tagRepository = tagRepository;
        }

        public IActionResult Index(string? categoryFilter = null, int page = 1, string sortOrder = "DateDesc")
        {
            const int PAGE_SIZE = 10;

            // Retrieve paged posts from your repository.
            List<Post> posts = _postRepository.GetPagedPosts(categoryFilter, page, PAGE_SIZE, sortOrder);

            // Map to the view model for the post preview
            List<PostPreviewVM> postPreviews = posts.Select(p => new PostPreviewVM
            {
                Id = p.Id,
                UserName = p.User.Username,
                Title = p.Title,
                Excerpt = p.Content != null && p.Content.Length > 100 
                    ? $"{p.Content.Substring(0, 100)}..." 
                    : p.Content ?? string.Empty,
                TimeAgo = TimeAgoHelper.GetTimeAgo(p.CreatedAt),
                LikeCount = p.Likes.Count,
                CommentCount = p.Comments.Count,
                IsPlaidEnabled = p.User.PlaidEnabled ?? false,
                PortfolioValueAtPosting = p.PortfolioValueAtPosting,
                ProfilePicture = p.User.ProfilePicture
            }).ToList();
            
            //            // For debugging 
//            foreach (var post in postPreviews)
//            {
//                _logger.LogInformation($"Post {post.Id} by {post.UserName}: PlaidEnabled={post.IsPlaidEnabled}, PortfolioValue={post.PortfolioValueAtPosting}");
//            }

            // Determine total pages
            int totalPosts = _postRepository.GetTotalPosts(categoryFilter);
            int totalPages = (int)Math.Ceiling((double)totalPosts / PAGE_SIZE);

            // Retrieve all tag names for the category filter
            List<string> tagNames = _tagRepository.GetAllTagNames();

            // Build the view model, including the current sort order
            var vm = new IndexVM
            {
                Posts = postPreviews,
                CurrentPage = page,
                TotalPages = totalPages,
                PagesToShow = PaginationHelper.GetPagination(page, totalPages, 7),
                SortOrder = sortOrder,
                Categories = tagNames,
                SelectedCategory = categoryFilter
            };

            return View(vm);
        }

        [Authorize]
        public async Task<IActionResult> Following(string? categoryFilter = null, int page = 1, string sortOrder = "DateDesc")
        {
            const int PAGE_SIZE = 10;

            // Get the current user's ID
            var identityId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(identityId))
            {
                _logger.LogWarning("User.Identity is null or NameIdentifier claim is missing.");
                return Unauthorized();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.IdentityId == identityId);
            if (user == null)
            {
                _logger.LogWarning("User not found for IdentityId: {IdentityId}", identityId);
                return NotFound();
            }

            int currentUserId = user.Id;

            // Retrieve paged posts from your repository.
            List<Post> posts = _postRepository.GetPagedPostsByUserFollows(currentUserId, categoryFilter, page, PAGE_SIZE, sortOrder);

            // Map to the view model for the post preview
            List<PostPreviewVM> postPreviews = posts.Select(p => new PostPreviewVM
            {
                Id = p.Id,
                UserName = p.User.Username,
                Title = p.Title,
                Excerpt = p.Content != null && p.Content.Length > 100 
                    ? $"{p.Content.Substring(0, 100)}..." 
                    : p.Content ?? string.Empty,
                TimeAgo = TimeAgoHelper.GetTimeAgo(p.CreatedAt),
                LikeCount = p.Likes.Count,
                CommentCount = p.Comments.Count,
                IsPlaidEnabled = p.User.PlaidEnabled ?? false,
                PortfolioValueAtPosting = p.PortfolioValueAtPosting,
                ProfilePicture = p.User.ProfilePicture
            }).ToList();

            // Determine total pages
            int totalPosts = _postRepository.GetTotalPostsByUserFollows(currentUserId, categoryFilter);
            int totalPages = (int)Math.Ceiling((double)totalPosts / PAGE_SIZE);

            // Retrieve all tag names for the category filter
            List<string> tagNames = _tagRepository.GetAllTagNames();

            // Build the view model, including the current sort order
            var vm = new IndexVM
            {
                CurrentUserId = currentUserId,
                CurrentUserName = User.Identity.IsAuthenticated ? User.Identity.Name : null,
                Posts = postPreviews,
                CurrentPage = page,
                TotalPages = totalPages,
                PagesToShow = PaginationHelper.GetPagination(page, totalPages, 7),
                SortOrder = sortOrder,
                Categories = tagNames,
                SelectedCategory = categoryFilter
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
