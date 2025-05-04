using TrustTrade.Models;
using TrustTrade.DAL.Abstract;
using TrustTrade.Services.Web.Interfaces;
using TrustTrade.ViewModels;
using TrustTrade.Helpers;

namespace TrustTrade.Services.Web.Implementations;

/// <summary>
/// Service for post-related operations.
/// </summary>
public class PostService : IPostService
{
    private readonly ILogger<PostService> _logger;
    private readonly IPostRepository _postRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IUserBlockRepository _userBlockRepository;
    private const int PAGE_SIZE = 10;
    private const int MAX_PAGES_TO_SHOW = 7;

    public PostService(ILogger<PostService> logger, IPostRepository postRepository, ITagRepository tagRepository, IUserBlockRepository userBlockRepository)
    {
        _logger = logger;
        _postRepository = postRepository;
        _tagRepository = tagRepository;
        _userBlockRepository = userBlockRepository;
    }

    public async Task<(List<PostPreviewVM> posts, int totalPosts)> GetPostPreviewsAsync(string? categoryFilter, int pageNumber, string sortOrder, int? currentUserId)
    {
        List<int>? blockedUserIds = await GetBlockedUserIds(currentUserId);
        var (posts, totalPosts) = await _postRepository.GetPagedPostsAsync(categoryFilter, pageNumber, PAGE_SIZE, sortOrder, blockedUserIds);

        return (MapPostsToPostPreviewVM(posts), totalPosts);
    }

    public async Task<(List<PostPreviewVM> posts, int totalPosts)> GetFollowingPostPreviewsAsync(int currentUserId, string? categoryFilter, int pageNumber, string sortOrder)
    {
        List<int> blockedUserIds = await _userBlockRepository.GetBlockedUserIdsAsync(currentUserId);
        var (posts, totalPosts) = await _postRepository.GetPagedPostsByUserFollowsAsync(currentUserId, categoryFilter, pageNumber, PAGE_SIZE, sortOrder, blockedUserIds);

        return (MapPostsToPostPreviewVM(posts), totalPosts);
    }

    public async Task<(List<PostPreviewVM> posts, int totalPosts)> GetUserPostPreviewsAsync(int userId, string? categoryFilter, int pageNumber, string sortOrder)
    {
        var (posts, totalPosts) = await _postRepository.GetPagedPostsByUserAsync(userId, categoryFilter, pageNumber, PAGE_SIZE, sortOrder);

        return (MapPostsToPostPreviewVM(posts), totalPosts);
    }

    public async Task<List<PostPreviewVM>> SearchPostsAsync(List<string> searchTerms, int? currentUserId)
    {
        List<int>? blockedUserIds = await GetBlockedUserIds(currentUserId);
        List<Post> posts = await _postRepository.SearchPostsAsync(searchTerms, blockedUserIds);

        return MapPostsToPostPreviewVM(posts);
    }

    public async Task<PostFiltersPartialVM> BuildPostFiltersAsync(string? categoryFilter, string sortOrder, string? searchQuery = null)
    {
        List<string> tagNames = await _tagRepository.GetAllTagNamesAsync();

        return new PostFiltersPartialVM
        {
            SelectedCategory = categoryFilter,
            SortOrder = sortOrder,
            Categories = tagNames,
            SearchQuery = searchQuery,
        };
    }

    public async Task<PaginationPartialVM> BuildPaginationAsync(string? categoryFilter, int pageNumber, int totalPosts, int? currentUserId)
    {
        List<int>? blockedUserIds = await GetBlockedUserIds(currentUserId);

        return MapToPaginationPartialVM(pageNumber, totalPosts, categoryFilter);
    }

    public async Task<PaginationPartialVM> BuildSearchPaginationAsync(string search, List<string> searchTerms, string? categoryFilter, int pageNumber)
    {
        //int totalPosts = await _postRepository.GetTotalPostsBySearchAsync(searchTerms, categoryFilter);

        // placeholder
        int totalPosts = 100; // Replace with actual logic to get total posts based on search terms

        return MapToPaginationPartialVM(pageNumber, totalPosts, categoryFilter, search);
    }

    private static List<PostPreviewVM> MapPostsToPostPreviewVM(List<Post> posts)
    {
        List<PostPreviewVM> postPreviews = new List<PostPreviewVM>();

        foreach (Post post in posts)
        {
            var isPlaidEnabled = post.User.PlaidEnabled ?? false;
            string? portfolioValue = null;

            // Retreive and format the portfolio value if Plaid is enabled
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

            postPreviews.Add(new PostPreviewVM
            {
                Id = post.Id,
                UserName = post.User.Username,
                Title = post.Title,
                Excerpt = post.Content != null && post.Content.Length > 100 
                    ? $"{post.Content.Substring(0, 100)}..." 
                    : post.Content ?? string.Empty,
                TimeAgo = TimeAgoHelper.GetTimeAgo(post.CreatedAt),
                LikeCount = post.Likes.Count,
                CommentCount = post.Comments.Count,
                IsPlaidEnabled = post.User.PlaidEnabled ?? false,
                PortfolioValueAtPosting = portfolioValue,
                ProfilePicture = post.User.ProfilePicture
            });
        }

        return postPreviews;
    }

    private static PaginationPartialVM MapToPaginationPartialVM(int currentPage, int totalPosts, string? categoryFilter, string? searchQuery = null)
    {
        int totalPages = (int)Math.Ceiling((double)totalPosts / PAGE_SIZE);

        return new PaginationPartialVM
        {
            CurrentPage = currentPage,
            TotalPages = totalPages,
            PagesToShow = PaginationHelper.GetPagination(currentPage, totalPages, MAX_PAGES_TO_SHOW),
            CategoryFilter = categoryFilter,
            SearchQuery = searchQuery,
        };
    }

    private async Task<List<int>?> GetBlockedUserIds(int? currentUserId)
    {
        if (currentUserId == null)
        {
            return null;
        }

        return await _userBlockRepository.GetBlockedUserIdsAsync(currentUserId.Value);
    }
}
