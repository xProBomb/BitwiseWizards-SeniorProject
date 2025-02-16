using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TrustTrade.Models;
using TrustTrade.DAL.Abstract;
using TrustTrade.ViewModels;
using TrustTrade.Helpers;

namespace TrustTrade.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IPostRepository _postRepository;

        public HomeController(ILogger<HomeController> logger, IPostRepository postRepository)
        {
            _logger = logger;
            _postRepository = postRepository;
        }

        public IActionResult Index(int page = 1)
        {
            const int PAGE_SIZE = 10;

            // Get posts for current page
            List<Post> posts = _postRepository.GetPagedPosts(page, PAGE_SIZE);

            List<PostPreviewVM> postPreviews = posts.Select(p => new PostPreviewVM
            {
                Id = p.Id,
                UserName = p.User.Username,
                Title = p.Title,
                Excerpt = (p.Content != null && p.Content.Length > 100) ? $"{p.Content.Substring(0, 100)}..." : (p.Content ?? string.Empty), // Display first 100 characters of content
                TimeAgo = TimeAgoHelper.GetTimeAgo(p.CreatedAt)
            }).ToList();

            // Determine total possible pages
            int totalPosts = _postRepository.GetTotalPosts();
            int totalPages = (int)Math.Ceiling((double)totalPosts / PAGE_SIZE);

            IndexVM vm = new()
            {
                Posts = postPreviews,
                CurrentPage = page,
                TotalPages = totalPages,
                PagesToShow = PaginationHelper.GetPagination(page, totalPages, 7)
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
