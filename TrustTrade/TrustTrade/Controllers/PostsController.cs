using Microsoft.AspNetCore.Mvc;
using TrustTrade.Models;
using TrustTrade.DAL.Abstract;
using TrustTrade.ViewModels;
using Microsoft.AspNetCore.Authorization;
using TrustTrade.Helpers;
using TrustTrade.Services.Web.Interfaces;
using TrustTrade.DAL.Concrete;

namespace TrustTrade.Controllers
{
    public class PostsController : Controller
    {
        private readonly ILogger<PostsController> _logger;
        private readonly IUserService _userService;
        private readonly IHoldingsRepository _holdingsRepository;
        private readonly IPostRepository _postRepository;
        private readonly ITagRepository _tagRepository;
        private readonly IPhotoRepository _photoRepository;

        public PostsController(
            ILogger<PostsController> logger,
            IUserService userService,
            IHoldingsRepository holdingsRepository,
            IPostRepository postRepository,
            ITagRepository tagRepository,
            IPhotoRepository photoRepository)
        {
            _logger = logger;
            _userService = userService;
            _holdingsRepository = holdingsRepository;
            _postRepository = postRepository;
            _tagRepository = tagRepository;
            _photoRepository = photoRepository;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Create()
        {
            // Retrieve all existing tags for the view model
            var vm = new CreatePostVM
            {
                ExistingTags = await _tagRepository.GetAllTagNamesAsync()
            };

            return View(vm);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create(CreatePostVM createPostVM)
        {
            if (!ModelState.IsValid)
            {
                // Repopulate the existing tags in case of validation errors
                createPostVM.ExistingTags = await _tagRepository.GetAllTagNamesAsync();
                return View(createPostVM);
            }

            User? user = await _userService.GetCurrentUserAsync(User);
            if (user == null) return Unauthorized();

            // Map the CreatePostVM to the Post entity
            var post = new Post
            {
                UserId = user.Id,
                Title = createPostVM.Title,
                Content = createPostVM.Content,
                IsPublic = createPostVM.IsPublic ?? false, // Should never be null due to validation
                                                           // PortfolioValueAtPosting will be set below if available
            };

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
                _logger.LogInformation(
                    $"User {user.Username} does not have Plaid enabled, skipping portfolio value calculation");
            }

            var base64Images = createPostVM.Photos
                .Where(photo => !string.IsNullOrWhiteSpace(photo))
                .SelectMany(photo => photo.Split(',', StringSplitOptions.RemoveEmptyEntries))
                .ToList();

            foreach (string base64Image in base64Images)
            {
                try
                {
                    string base64Data = base64Image;

                    // Remove any data URI scheme prefix if present
                    if (base64Data.StartsWith("data:image"))
                    {
                        base64Data = base64Data.Substring(base64Data.IndexOf(",") + 1);
                    }

                    // Validate and decode Base64
                    byte[] imageBytes = Convert.FromBase64String(base64Data);

                    var photo = new Photo
                    {
                        Image = imageBytes,
                        Post = post,
                        CreatedAt = DateTime.Now
                    };

                    await _photoRepository.AddOrUpdateAsync(photo);
                }
                catch (FormatException fe)
                {
                    _logger.LogWarning(fe, "Invalid Base64 image skipped.");
                    continue; // Just skip bad entries
                }
            }

            // Add the selected tags to the post
            foreach (string tagName in createPostVM.SelectedTags)
            {
                Tag? tag = await _tagRepository.FindByTagNameAsync(tagName);
                if (tag == null)
                {
                    // If the tag doesn't exist, create it and add it to the database
                    tag = new Tag { TagName = tagName };
                    await _tagRepository.AddOrUpdateAsync(tag);
                }

                // Add the tag to the post and the post to the tag
                post.Tags.Add(tag);
                tag.Posts.Add(post);
            }

            // Save the post to the database
            await _postRepository.AddOrUpdateAsync(post);
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Details(int id)
        {
            // Retrieve the post from the repository
            Post? post = await _postRepository.FindByIdAsync(id);
            if (post == null) return NotFound();

            // Get current logged-in user ONCE
            User? user = await _userService.GetCurrentUserAsync(User);

            var isPlaidEnabled = post.User?.PlaidEnabled ?? false;
            string? portfolioValue = null;

           

            // Retrieve and format the portfolio value if Plaid is enabled
            if (isPlaidEnabled)
            {
                if (post.PortfolioValueAtPosting.HasValue)
                {
                    portfolioValue = FormatCurrencyAbbreviate.FormatCurrencyAbbreviated(post.PortfolioValueAtPosting.Value);
                }
                else
                {
                    portfolioValue = "$0";
                }
            }

            bool isOwnedByCurrentUser = user != null && user.Id == post.UserId;
            bool isLikedByCurrentUser = user != null && post.Likes.Any(l => l.UserId == user.Id);

            // Convert comments to view model
            List<CommentVM> comments = post.Comments.Select(comment =>
            {
                string? commentPortfolioValue = null;

                if (comment.User?.PlaidEnabled == true)
                {
                    commentPortfolioValue = comment.PortfolioValueAtPosting.HasValue
                        ? FormatCurrencyAbbreviate.FormatCurrencyAbbreviated(comment.PortfolioValueAtPosting.Value)
                        : "$0";
                }
                 if (user != null && user.Is_Suspended == true)
                    {
                        post.User.ProfilePicture = Array.Empty<byte>(); // Set to default image unless justin tells me a better way
                    }

                return new CommentVM
                {
                    Id = comment.Id,
                    Username = comment.User?.Username ?? "Unknown",
                    Content = comment.Content,
                    TimeAgo = TimeAgoHelper.GetTimeAgo(comment.CreatedAt),
                    IsPlaidEnabled = comment.User?.PlaidEnabled ?? false,
                    PortfolioValueAtPosting = commentPortfolioValue,
                    ProfilePicture = comment.User?.Is_Suspended == true ? Array.Empty<byte>(): comment.User?.ProfilePicture,
                    IsOwnedByCurrentUser = user != null && comment.UserId == user.Id,
                    LikeCount = comment.CommentLikes?.Count ?? 0,
                    IsLikedByCurrentUser = user != null && comment.CommentLikes?.Any(l => l.UserId == user.Id) == true,
                };
            }).ToList();

            List<string> photos = (await _photoRepository.GetPhotosByPostIdAsync(post.Id))
                .Where(photo => photo?.Image != null && photo.Image.Length > 0)
                .Select(photo => $"data:image/jpeg;base64,{Convert.ToBase64String(photo.Image)}")
                .ToList();

            if (photos.Count == 0)
            {
                _logger.LogInformation($"No photos found for post {post.Id}");
            }

            // Ensure previous profile picture is set to default if user is suspended
           
            if (post.User?.Is_Suspended == true)
            {
                post.User.ProfilePicture = Array.Empty<byte>();
            }

            // Map to ViewModel
            var vm = new PostDetailsVM
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                Photos = photos,
                Username = post.User?.Username ?? "Unknown",
                TimeAgo = TimeAgoHelper.GetTimeAgo(post.CreatedAt),
                Tags = post.Tags?.Select(t => t.TagName).ToList() ?? new List<string>(),
                LikeCount = post.Likes?.Count ?? 0,
                IsLikedByCurrentUser = isLikedByCurrentUser,
                CommentCount = post.Comments?.Count ?? 0,
                IsPlaidEnabled = isPlaidEnabled,
                PortfolioValueAtPosting = portfolioValue,
                IsOwnedByCurrentUser = isOwnedByCurrentUser,
                IsUserAdmin = user?.IsAdmin ?? false,
                ProfilePicture = post.User?.ProfilePicture,
                Comments = comments,
                IsSavedByCurrentUser = user?.SavedPosts?.Any(sp => sp.PostId == post.Id) ?? false,
            };

            return View(vm);
        }


        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            Post? post = await _postRepository.FindByIdAsync(id);
            if (post == null) return NotFound();

            User? user = await _userService.GetCurrentUserAsync(User);
            if (user == null) return Unauthorized();

            // Ensure the user is the author of the post
            if (post.UserId != user.Id) return Unauthorized();

            var vm = new PostEditVM
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                IsPublic = post.IsPublic,
                AvailableTags = await _tagRepository.GetAllTagNamesAsync(),
                SelectedTags = post.Tags.Select(t => t.TagName).ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Edit(int id, PostEditVM postEditVM)
        {
            if (!ModelState.IsValid)
            {
                postEditVM.AvailableTags = await _tagRepository.GetAllTagNamesAsync();
                return View(postEditVM);
            }

            if (id != postEditVM.Id) return BadRequest("Post ID mismatch");

            Post? post = await _postRepository.FindByIdAsync(id);
            if (post == null) return NotFound();

            User? user = await _userService.GetCurrentUserAsync(User);
            if (user == null) return Unauthorized();

            // Ensure the user is the author of the post
            if (post.UserId != user.Id) return Unauthorized();

            // Update the post with the new values
            post.Title = postEditVM.Title;
            post.Content = postEditVM.Content;
            post.IsPublic = postEditVM.IsPublic;

            // Clear existing post tags and add the new ones
            post.Tags.Clear();
            foreach (string tagName in postEditVM.SelectedTags)
            {
                Tag? tag = await _tagRepository.FindByTagNameAsync(tagName);
                if (tag == null)
                {
                    // If the tag doesn't exist, create it and add it to the database
                    tag = new Tag { TagName = tagName };
                    await _tagRepository.AddOrUpdateAsync(tag);
                }

                post.Tags.Add(tag);
            }

            // Save the updated post
            await _postRepository.AddOrUpdateAsync(post);
            return RedirectToAction("Details", new { id = post.Id });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            Post? post = await _postRepository.FindByIdAsync(id);
            if (post == null) return NotFound();

            User? user = await _userService.GetCurrentUserAsync(User);
            if (user == null) return Unauthorized();

            // Ensure the user is the author of the post
            if (post.UserId != user.Id) return Unauthorized();

            await _postRepository.DeleteAsync(post);

            return RedirectToAction("Index", "Home");
        }
    }
}