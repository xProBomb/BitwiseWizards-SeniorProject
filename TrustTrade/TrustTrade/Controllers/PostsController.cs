using Microsoft.AspNetCore.Mvc;
using TrustTrade.Models;
using TrustTrade.DAL.Abstract;
using TrustTrade.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace TrustTrade.Controllers
{
    public class PostsController : Controller
    {
        private readonly ILogger<PostsController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHoldingsRepository _holdingsRepository;
        private readonly IPostRepository _postRepository;
        private readonly ITagRepository _tagRepository;
        private readonly IUserRepository _userRepository;

        public PostsController(
            ILogger<PostsController> logger,
            UserManager<IdentityUser> userManager,
            IHoldingsRepository holdingsRepository,
            IPostRepository postRepository,
            ITagRepository tagRepository,
            IUserRepository userRepository)
        {
            _logger = logger;
            _userManager = userManager;
            _holdingsRepository = holdingsRepository;
            _postRepository = postRepository;
            _tagRepository = tagRepository;
            _userRepository = userRepository;
        }

        [Authorize]
        [HttpGet]
        public IActionResult Create()
        {
            // Retrieve all existing tags for the view model
            var vm = new CreatePostVM
            {
                ExistingTags = _tagRepository.GetAllTagNames()
            };

            return View(vm);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(CreatePostVM createPostVM)
        {
            if (ModelState.IsValid)
            {
                string? identityUserId = _userManager.GetUserId(User);
                if (identityUserId == null)
                {
                    return Unauthorized();
                }

                // Retrieve the user from the database
                User? user = await _userRepository.FindByIdentityIdAsync(identityUserId);
                if (user == null)
                {
                    return NotFound();
                }

                // Map the CreatePostVM to the Post entity
                var post = new Post
                {
                    UserId = user.Id,
                    Title = createPostVM.Title,
                    Content = createPostVM.Content,
                    // PortfolioValueAtPosting will be set below if available
                };

                // Add the selected tags to the post
                foreach (string tagName in createPostVM.SelectedTags)
                {
                    Tag? tag = _tagRepository.FindByTagName(tagName);
                    if (tag != null)
                    {
                        // Add the tag to the post and the post to the tag
                        post.Tags.Add(tag);
                        tag.Posts.Add(post);
                    }
                }

                // Only attempt to refresh and calculate portfolio value if Plaid is enabled
                if (user.PlaidEnabled == true)
                {
                    try
                    {
                        // Refresh the user's holdings using the repository method
                        bool refreshSuccess = await _holdingsRepository.RefreshHoldingsAsync(user.Id);

                        if (refreshSuccess)
                        {
                            // Get the latest holdings after refresh
                            var holdings = await _holdingsRepository.GetHoldingsForUserAsync(user.Id);

                            // Calculate total portfolio value by summing (quantity * current price)
                            decimal totalPortfolioValue = 0;
                            foreach (var holding in holdings)
                            {
                                totalPortfolioValue += holding.Quantity * holding.CurrentPrice;
                            }

                            // Set the portfolio value in the post
                            post.PortfolioValueAtPosting = totalPortfolioValue;

                            // Log the portfolio value for debugging
                            _logger.LogInformation(
                                $"User {user.Username} portfolio value at posting: {totalPortfolioValue:C}");
                        }
                        else
                        {
                            // Log warning if refresh fails but continue with post creation
                            _logger.LogWarning(
                                $"Failed to refresh holdings for user {user.Username} during post creation");
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log error but don't prevent post creation
                        _logger.LogError(ex, $"Error calculating portfolio value for user {user.Username}");
                    }
                }
                else
                {
                    // Log that we're skipping portfolio calculation since Plaid isn't enabled
                    _logger.LogInformation(
                        $"User {user.Username} does not have Plaid enabled, skipping portfolio value calculation");
                }

                // Save the post to the database
                _postRepository.AddOrUpdate(post);
                return RedirectToAction("Index", "Home");
            }

            // Repopulate the existing tags in case of validation errors
            createPostVM.ExistingTags = _tagRepository.GetAllTagNames();
            return View(createPostVM);
        }
    }
}