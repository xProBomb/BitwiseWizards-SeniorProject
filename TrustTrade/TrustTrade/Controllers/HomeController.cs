using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TrustTrade.Models;
using TrustTrade.DAL.Abstract;
using TrustTrade.ViewModels;
using TrustTrade.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace TrustTrade.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IPostRepository _postRepository;
        private readonly ITagRepository _tagRepository;
        private readonly IUserRepository _userRepository;

        public HomeController(
            ILogger<HomeController> logger, 
            UserManager<IdentityUser> userManager, 
            IPostRepository postRepository, 
            ITagRepository tagRepository, 
            IUserRepository userRepository)
        {
            _logger = logger;
            _userManager = userManager;
            _postRepository = postRepository;
            _tagRepository = tagRepository;
            _userRepository = userRepository;
        }

        public IActionResult Index(string? categoryFilter = null, int page = 1, string sortOrder = "DateDesc")
        {
            const int PAGE_SIZE = 10;

            // Retrieve paged posts from your repository.
            List<Post> posts = _postRepository.GetPagedPosts(categoryFilter, page, PAGE_SIZE, sortOrder);

            // Map to your view model for the post preview
            List<PostPreviewVM> postPreviews = posts.Select(p => new PostPreviewVM
            {
                Id = p.Id,
                UserName = p.User.Username,
                Title = p.Title,
                Excerpt = p.Content != null && p.Content.Length > 100 
                    ? $"{p.Content.Substring(0, 100)}..." 
                    : p.Content ?? string.Empty,
                TimeAgo = TimeAgoHelper.GetTimeAgo(p.CreatedAt),
                IsPlaidEnabled = p.User.PlaidEnabled ?? false,
                PortfolioValueAtPosting = p.PortfolioValueAtPosting
            }).ToList();
            
            // For debugging 
            foreach (var post in postPreviews)
            {
                _logger.LogInformation($"Post {post.Id} by {post.UserName}: PlaidEnabled={post.IsPlaidEnabled}, PortfolioValue={post.PortfolioValueAtPosting}");
            }

            // Determine total pages
            int totalPosts = _postRepository.GetTotalPosts(categoryFilter);
            int totalPages = (int)Math.Ceiling((double)totalPosts / PAGE_SIZE);

            // Retrieve all tag names for the category filter
            List<string> tagNames = _tagRepository.GetAllTagNames();

            // Build the view model, including the current sort order
            IndexVM vm = new()
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
        [HttpGet]
        public IActionResult CreatePost()
        {
            // Retrieve all existing tags for the view model
            CreatePostVM vm = new CreatePostVM
            {
                ExistingTags = _tagRepository.GetAllTagNames()
            };

            return View(vm);
        }

        [Authorize]        
        [HttpPost]
        public IActionResult CreatePost(CreatePostVM createPostVM)
        {
            if (ModelState.IsValid)
            {
                string? identityUserId = _userManager.GetUserId(User);
                if (identityUserId == null)
                {
                    return Unauthorized();
                }

                // Retrieve the user from the database
                User? user = _userRepository.FindByIdentityId(identityUserId);
                if (user == null)
                {
                    return NotFound();
                }

                // Map the CreatePostVM to the Post entity
                Post post = new Post
                {
                    UserId = user.Id,
                    Title = createPostVM.Title,
                    Content = createPostVM.Content
                };

                // Add the selected tags to the post
                foreach (string tagName in createPostVM.SelectedTags)
                {
                    Tag? tag = _tagRepository.GetTagByName(tagName);
                    if (tag != null)
                    {
                        // Add the tag to the post and the post to the tag
                        post.Tags.Add(tag);
                        tag.Posts.Add(post);
                    }
                }

                // Save the post to the database
                _postRepository.AddOrUpdate(post);
                return RedirectToAction("Index");
            }

            // Repopulate the existing tags in case of validation errors
            createPostVM.ExistingTags = _tagRepository.GetAllTagNames();
            return View(createPostVM);
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
