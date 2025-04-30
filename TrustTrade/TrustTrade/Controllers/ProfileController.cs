using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrustTrade.DAL.Abstract;
using TrustTrade.Models;
using TrustTrade.Models.ViewModels;
using TrustTrade.Services;
using TrustTrade.ViewModels;
using TrustTrade.Services.Web.Interfaces;

namespace TrustTrade.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly TrustTradeDbContext _context;
        private readonly IHoldingsRepository _holdingsRepository;
        private readonly ILogger<ProfileController> _logger;
        private readonly IPostService _postService;
        private readonly IProfileService _profileService;
        private readonly IUserBlockRepository _userBlockRepository;
        private readonly IUserService _userService;
        private readonly IPerformanceScoreRepository _performanceScoreRepository;
        private readonly INotificationService _notificationService;

        public ProfileController(
            TrustTradeDbContext context,
            IHoldingsRepository holdingsRepository,
            ILogger<ProfileController> logger,
            IPostService postService,
            IProfileService profileService,
            IUserBlockRepository userBlockRepository,
            IUserService userService,
            IPerformanceScoreRepository performanceScoreRepository,
            INotificationService notificationService)
        {
            _context = context;
            _holdingsRepository = holdingsRepository;
            _logger = logger;
            _postService = postService;
            _profileService = profileService;
            _userBlockRepository = userBlockRepository;
            _userService = userService;
            _performanceScoreRepository = performanceScoreRepository;
            _notificationService = notificationService;
        }

        // route to get to logged in users profile "/Profile"
        // This is the method for accessing the owner's profile
        [HttpGet("/Profile")]
        public async Task<IActionResult> MyProfile()
        {
            var identityId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(identityId))
            {
                return Unauthorized();
            }

            var user = await _context.Users
                .Include(u => u.FollowerFollowerUsers)
                .Include(u => u.FollowerFollowingUsers)
                .FirstOrDefaultAsync(u => u.IdentityId == identityId);

            if (user == null)
            {
                return NotFound();
            }

            // Get visibility settings
            var visibilitySettings = await _context.PortfolioVisibilitySettings
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            bool hideDetails = false;
            bool hideAll = false;

            if (visibilitySettings != null)
            {
                hideDetails = visibilitySettings.HideDetailedInformation;
                hideAll = visibilitySettings.HideAllPositions;
            }

            var holdings = await _holdingsRepository.GetHoldingsForUserAsync(user.Id);
            
            var holdingViewModels = holdings.Select(h => new HoldingViewModel
            {
                Symbol = h.Symbol,
                Quantity = h.Quantity,
                CurrentPrice = h.CurrentPrice,
                CostBasis = h.CostBasis,
                Institution = h.PlaidConnection.InstitutionName,
                TypeOfSecurity = h.TypeOfSecurity,
                IsHidden = hideAll || h.IsHidden
            }).ToList();
            
            var (score, isRated, breakdown) = await _performanceScoreRepository.CalculatePerformanceScoreAsync(user.Id);
            
            var model = new ProfileViewModel
            {
                IdentityId = user.IdentityId,
                Username = user.Username,
                CreatedAt = user.CreatedAt,
                Bio = user.Bio,
                IsVerified = user.IsVerified ?? false,
                PlaidEnabled = user.PlaidEnabled ?? false,
                LastPlaidSync = user.LastPlaidSync,
                FollowersCount = user.FollowerFollowerUsers?.Count ?? 0,
                FollowingCount = user.FollowerFollowingUsers?.Count ?? 0,
                Followers = user.FollowerFollowerUsers?.Select(f => f.FollowingUser.Username).ToList() ??
                            new List<string>(),
                Following = user.FollowerFollowingUsers?.Select(f => f.FollowerUser.Username).ToList() ??
                            new List<string>(),
                Holdings = holdingViewModels,
                LastHoldingsUpdate = holdings.Any() ? holdings.Max(h => h.LastUpdated) : null,
                UserTag = user.UserTag,
                IsFollowing = false,
                HideDetailedInformation = hideDetails,
                HideAllPositions = hideAll,
                PerformanceScore = score,
                HasRatedScore = isRated,
                ScoreBreakdown = breakdown,
                ProfilePicture = user.ProfilePicture
            };

            return View("Profile", model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfilePicture(IFormFile ProfilePicture)
        {
            if (ProfilePicture == null || !ProfilePicture.ContentType.StartsWith("image/"))
            {
                TempData["ProfilePictureError"] = "Invalid file type. Please upload a valid image.";
                return RedirectToAction("MyProfile");
            }

            // Process the valid image file (e.g., save it to the database or file system)
            using (var memoryStream = new MemoryStream())
            {
                ProfilePicture.CopyTo(memoryStream);
                var imageData = memoryStream.ToArray();

                // Save the image data to the user's profile (example)
                var identityId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var user = await _context.Users.FirstOrDefaultAsync(u => u.IdentityId == identityId);
                user.ProfilePicture = imageData;
                _context.Update(user);
                await _context.SaveChangesAsync();
                // delay to ensure the image is saved
                await Task.Delay(1000);
            }

            TempData["ProfilePictureSuccess"] = "Profile picture updated successfully.";
            return RedirectToAction("MyProfile");
        }

        // In order to access the profile of a user, use the route below
        // This is the method for accessing a non-owners profile
        [AllowAnonymous]
        [HttpGet("/Profile/User/{username}", Name = "UserProfileRoute")]
        public async Task<IActionResult> UserProfile(string username)
        {
    
    
    var identityId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    _logger.LogDebug("Current Identity ID: {IdentityId}", identityId);

    if (string.IsNullOrEmpty(username))
    {
        return RedirectToAction(nameof(MyProfile));
    }

    var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.IdentityId == identityId);
    var currentUserId = currentUser?.Id;
    _logger.LogDebug("Current User ID: {CurrentUserId}", currentUserId);


    var user = await _context.Users
        .Include(u => u.FollowerFollowerUsers)
        .Include(u => u.FollowerFollowingUsers)
        .FirstOrDefaultAsync(u => u.Username == username);

    if (user.Is_Suspended == true)
    { 
        var suspendedUserModel = new ProfileViewModel
        {
            Id = user.Id,
            IdentityId = user.IdentityId,
            Username = user.Username,
        };

        return View("Profile", suspendedUserModel);
    }
    if (user == null)
    {
        _logger.LogDebug("User not found: {Username}", username);
        return NotFound();
    }

    _logger.LogDebug("Viewing Profile User ID: {UserId}", user.Id);

    var holdings = await _holdingsRepository.GetHoldingsForUserAsync(user.Id);
    var holdingViewModels = holdings.Select(h => new HoldingViewModel
    {
        Symbol = h.Symbol,
        Quantity = h.Quantity,
        CurrentPrice = h.CurrentPrice,
        CostBasis = h.CostBasis,
        Institution = h.PlaidConnection.InstitutionName,
        TypeOfSecurity = h.TypeOfSecurity,
        IsHidden = h.IsHidden
    }).ToList();

    var visibilitySettings = await _context.PortfolioVisibilitySettings
        .FirstOrDefaultAsync(p => p.UserId == user.Id);

    bool hideDetails = false;
    bool hideAll = false;

    if (visibilitySettings != null)
    {
        hideDetails = visibilitySettings.HideDetailedInformation;
        hideAll = visibilitySettings.HideAllPositions;
    }

    var filteredHoldings = holdingViewModels.Where(h => !hideAll || !h.IsHidden).ToList();
    
    var (score, isRated, breakdown) = await _performanceScoreRepository.CalculatePerformanceScoreAsync(user.Id);

    var blockedUserIds = await _userBlockRepository.GetBlockedUserIdsAsync(currentUserId ?? 0);
    var isBlocked = blockedUserIds.Contains(user.Id);

    var model = new ProfileViewModel
    {
        Id = user.Id,
        IdentityId = user.IdentityId,
        Username = user.Username,
        CreatedAt = user.CreatedAt,
        Bio = user.Bio,
        IsVerified = user.IsVerified ?? false,
        PlaidEnabled = user.PlaidEnabled ?? false,
        LastPlaidSync = user.LastPlaidSync,
        FollowersCount = user.FollowerFollowerUsers?.Count ?? 0,
        FollowingCount = user.FollowerFollowingUsers?.Count ?? 0,
        Followers = user.FollowerFollowerUsers?.Select(f => f.FollowingUser.Username).ToList() ??
                    new List<string>(),
        Following = user.FollowerFollowingUsers?.Select(f => f.FollowerUser.Username).ToList() ??
                    new List<string>(),
        Holdings = filteredHoldings,
        LastHoldingsUpdate = holdings.Any() ? holdings.Max(h => h.LastUpdated) : null,
        UserTag = user.UserTag,
        IsFollowing = user.FollowerFollowerUsers?.Any(f => f.FollowingUserId == currentUserId) ?? false,
        PerformanceScore = score,
        HasRatedScore = isRated,
        ScoreBreakdown = breakdown,
        ProfilePicture = user.ProfilePicture,
        IsBlocked = isBlocked,
        // property so we know whether to show the message button
        CanMessage = currentUserId.HasValue && currentUserId != user.Id
    };

    return View("Profile", model);
}

        [HttpPost]
        public async Task<IActionResult> RefreshHoldings()
        {
            try
            {
                var identityId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(identityId))
                {
                    return Unauthorized();
                }

                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.IdentityId == identityId);

                if (user == null)
                {
                    return NotFound();
                }

                var success = await _holdingsRepository.RefreshHoldingsAsync(user.Id);
                if (!success)
                {
                    return StatusCode(500, new { error = "Failed to refresh holdings" });
                }

                return RedirectToAction(nameof(MyProfile));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing holdings");
                return StatusCode(500, new { error = "An unexpected error occurred" });
            }
        }

        /// <summary>
        /// Displays the portfolio visibility settings page
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> PortfolioVisibilitySettings()
        {
            var identityId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(identityId))
            {
                return Unauthorized();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.IdentityId == identityId);

            if (user == null)
            {
                return NotFound();
            }

            // Get user's visibility settings
            var settings = await _context.PortfolioVisibilitySettings
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (settings == null)
            {
                // Create default settings if none exist
                settings = new PortfolioVisibilitySettings
                {
                    UserId = user.Id,
                    HideDetailedInformation = false,
                    HideAllPositions = false
                };
                _context.PortfolioVisibilitySettings.Add(settings);
                await _context.SaveChangesAsync();
            }

            // Get user's holdings
            var holdings = await _context.InvestmentPositions
                .Include(i => i.PlaidConnection)
                .Where(i => i.PlaidConnection.UserId == user.Id)
                .ToListAsync();

            var viewModel = new PortfolioVisibilityViewModel
            {
                HideDetailedInformation = settings.HideDetailedInformation,
                HideAllPositions = settings.HideAllPositions,
                Holdings = holdings.Select(h => new HoldingVisibilityViewModel
                {
                    Id = h.Id,
                    Symbol = h.Symbol,
                    IsHidden = h.IsHidden
                }).ToList()
            };

            return View(viewModel);
        }

        /// <summary>
        /// Handles saving portfolio visibility settings
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SavePortfolioVisibilitySettings(PortfolioVisibilityViewModel model)
        {
            var identityId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(identityId))
            {
                return Unauthorized();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.IdentityId == identityId);

            if (user == null)
            {
                return NotFound();
            }

            // Update global visibility settings
            var settings = await _context.PortfolioVisibilitySettings
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (settings == null)
            {
                settings = new PortfolioVisibilitySettings
                {
                    UserId = user.Id
                };
                _context.PortfolioVisibilitySettings.Add(settings);
            }

            settings.HideDetailedInformation = model.HideDetailedInformation;
            settings.HideAllPositions = model.HideAllPositions;
            settings.LastUpdated = DateTime.UtcNow;

            // Update individual holding visibility
            if (model.Holdings != null)
            {
                foreach (var holdingModel in model.Holdings)
                {
                    var holding = await _context.InvestmentPositions
                        .Include(i => i.PlaidConnection)
                        .FirstOrDefaultAsync(i => i.Id == holdingModel.Id && i.PlaidConnection.UserId == user.Id);

                    if (holding != null)
                    {
                        // If "Hide All" is enabled, hide all positions
                        // Otherwise, use the individual setting
                        holding.IsHidden = model.HideAllPositions || holdingModel.IsHidden;
                    }
                }
            }

            await _context.SaveChangesAsync();

            // Redirect back to profile
            return RedirectToAction(nameof(MyProfile));
        }

        /// <summary>
        /// Updates the user's profile information
        /// </summary>
        /// <param name="bio">The user's updated bio</param>
        /// <param name="userTag">The user's selected trading preference</param>
        /// <returns>Redirects to Profile page</returns>
        [HttpPost]
        public async Task<IActionResult> UpdateProfile(string username, string bio, string userTag)
        {
            try
            {
                var identityId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(identityId))
                    return Unauthorized();

                var user = await _context.Users.FirstOrDefaultAsync(u => u.IdentityId == identityId);
                if (user == null)
                    return NotFound();

                // Check uniqueness if the username has changed.
                if (!string.Equals(user.Username, username, StringComparison.OrdinalIgnoreCase))
                {
                    bool usernameExists = await _context.Users
                        .AnyAsync(u => u.Username == username && u.IdentityId != identityId);

                    if (usernameExists)
                    {
                        // Set a custom error message in the ViewBag.
                        ViewBag.UsernameError = $"The username {username} is already taken.";

                        // Rebuild the complete ProfileViewModel using the current data from the database
                        var updatedUser = await _context.Users
                            .Include(u => u.FollowerFollowerUsers)
                            .Include(u => u.FollowerFollowingUsers)
                            .FirstOrDefaultAsync(u => u.IdentityId == identityId);

                        var model = new ProfileViewModel
                        {
                            IdentityId = updatedUser.IdentityId,
                            Username = updatedUser.Username, // Keep the original username.
                            Bio = updatedUser.Bio,           // Keep the original bio.
                            UserTag = updatedUser.UserTag,   // Keep the original trading preference.
                            CreatedAt = updatedUser.CreatedAt,
                            FollowersCount = updatedUser.FollowerFollowerUsers?.Count ?? 0,
                            FollowingCount = updatedUser.FollowerFollowingUsers?.Count ?? 0,
                            Followers = updatedUser.FollowerFollowerUsers?.Select(f => f.FollowingUser.Username).ToList() ?? new List<string>(),
                            Following = updatedUser.FollowerFollowingUsers?.Select(f => f.FollowerUser.Username).ToList() ?? new List<string>(),
                            IsFollowing = false,
                            HideDetailedInformation = false,
                            HideAllPositions = false,
                            PerformanceScore = 0,
                            HasRatedScore = false,
                            ScoreBreakdown = new Dictionary<string, decimal>(),
                            ProfilePicture = updatedUser.ProfilePicture
                        };

                        return View("Profile", model);
                    }
                }

                // Otherwise, update and save the new values.
                user.Username = username;
                user.Bio = bio;
                user.UserTag = userTag;
                _context.Update(user);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(MyProfile));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile");
                return StatusCode(500, new { error = "An unexpected error occurred" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Follow(string profileId)
        {
            var identityId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(identityId))
            {
                return Unauthorized();
            }

            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.IdentityId == identityId);
            if (currentUser == null)
            {
                return NotFound();
            }

            var userToFollow = await _context.Users.FirstOrDefaultAsync(u => u.IdentityId == profileId);
            if (userToFollow == null)
            {
                return NotFound();
            }

            var follower = new Follower
            {
                FollowerUserId = userToFollow.Id,
                FollowingUserId = currentUser.Id,
                CreatedAt = DateTime.UtcNow
            };

            _context.Followers.Add(follower);
            await _context.SaveChangesAsync();
            
            // Create notification for the followed user
            await _notificationService.CreateFollowNotificationAsync(currentUser.Id, userToFollow.Id);

            return RedirectToAction("UserProfile", new { username = userToFollow.Username });
        }

        [HttpPost]
        public async Task<IActionResult> Unfollow(string profileId)
        {
            var identityId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(identityId))
            {
                return Unauthorized();
            }

            var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.IdentityId == identityId);
            if (currentUser == null)
            {
                return NotFound();
            }

            var userToUnfollow = await _context.Users.FirstOrDefaultAsync(u => u.IdentityId == profileId);
            if (userToUnfollow == null)
            {
                return NotFound();
            }

            var follower = await _context.Followers
                .FirstOrDefaultAsync(f => f.FollowingUserId == currentUser.Id && f.FollowerUserId == userToUnfollow.Id);

            if (follower != null)
            {
                _context.Followers.Remove(follower);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("UserProfile", new { username = userToUnfollow.Username });
        }

        [HttpPost]
        public async Task<IActionResult> Block(string profileId)
        {
            var currentUser = await _userService.GetCurrentUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var userToBlock = await _context.Users.FirstOrDefaultAsync(u => u.IdentityId == profileId);
            if (userToBlock == null)
            {
                return NotFound();
            }

            var block = new UserBlock
            {
                BlockedId = userToBlock.Id,
                BlockerId = currentUser.Id,
                BlockedAt = DateTime.UtcNow
            };

            _context.UserBlocks.Add(block);
            await _context.SaveChangesAsync();

            return RedirectToAction("UserProfile", new { username = userToBlock.Username });

        }

        [HttpPost]
        public async Task<IActionResult> Unblock(string profileId)
        {
            var currentUser = await _userService.GetCurrentUserAsync(User);
            if (currentUser == null)
            {
                return Unauthorized();
            }

            var userToUnblock = await _context.Users.FirstOrDefaultAsync(u => u.IdentityId == profileId);
            if (userToUnblock == null)
            {
                return NotFound();
            }

            var block = await _context.UserBlocks
                .FirstOrDefaultAsync(b => b.BlockerId == currentUser.Id && b.BlockedId == userToUnblock.Id);

            if (block != null)
            {
                _context.UserBlocks.Remove(block);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("UserProfile", new { username = userToUnblock.Username });
        }

        [AllowAnonymous]
        [HttpGet("/Profile/User/{username}/Posts")]
        public async Task<IActionResult> UserPosts(string username, string? categoryFilter = null, int pageNumber = 1, string sortOrder = "DateDesc")
        {
            var user = await _userService.GetUserByUsernameAsync(username);
            if (user == null) return NotFound();

            var postPreviews = await _postService.GetUserPostPreviewsAsync(user.Id, categoryFilter, pageNumber, sortOrder);
            var postFiltersVM = await _postService.BuildPostFiltersAsync(categoryFilter, sortOrder);
            var paginationVM = await _postService.BuildUserPaginationAsync(user.Id, categoryFilter, pageNumber);

            var vm = new UserPostsVM
            {
                Posts = postPreviews,
                Pagination = paginationVM,
                PostFilters = postFiltersVM,
            };

            return View(vm);
        }
    }
}