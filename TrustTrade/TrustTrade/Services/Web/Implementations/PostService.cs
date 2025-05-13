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
    private readonly ISavedPostRepository _savedPostRepository;
    private const int PAGE_SIZE = 10;
    private const int MAX_PAGES_TO_SHOW = 7;

    public PostService(ILogger<PostService> logger, IPostRepository postRepository, ITagRepository tagRepository, IUserBlockRepository userBlockRepository, ISavedPostRepository savedPostRepository)
    {
        _logger = logger;
        _postRepository = postRepository;
        _tagRepository = tagRepository;
        _userBlockRepository = userBlockRepository;
        _savedPostRepository = savedPostRepository;
    }

    public async Task<(List<Post> posts, int totalPosts)> GetPagedPostsAsync(string? categoryFilter, int pageNumber, string sortOrder, int? currentUserId)
    {
        List<int>? blockedUserIds = await GetBlockedUserIds(currentUserId);
        var (posts, totalPosts) = await _postRepository.GetPagedPostsAsync(categoryFilter, pageNumber, PAGE_SIZE, sortOrder, blockedUserIds);

        return (posts, totalPosts);
    }

    public async Task<(List<Post> posts, int totalPosts)> GetFollowingPagedPostsAsync(int currentUserId, string? categoryFilter, int pageNumber, string sortOrder)
    {
        List<int> blockedUserIds = await _userBlockRepository.GetBlockedUserIdsAsync(currentUserId);
        var (posts, totalPosts) = await _postRepository.GetPagedPostsByUserFollowsAsync(currentUserId, categoryFilter, pageNumber, PAGE_SIZE, sortOrder, blockedUserIds);

        return (posts, totalPosts);
    }

    public async Task<(List<Post> posts, int totalPosts)> GetUserPagedPostsAsync(int userId, string? categoryFilter, int pageNumber, string sortOrder)
    {
        var (posts, totalPosts) = await _postRepository.GetPagedPostsByUserAsync(userId, categoryFilter, pageNumber, PAGE_SIZE, sortOrder);

        return (posts, totalPosts);
    }

    public async Task<List<Post>> SearchPostsAsync(List<string> searchTerms, int? currentUserId)
    {
        List<int>? blockedUserIds = await GetBlockedUserIds(currentUserId);
        List<Post> posts = await _postRepository.SearchPostsAsync(searchTerms, blockedUserIds);

        return posts;
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

    public async Task AddPostToSavedPostsAsync(int postId, int userId)
    {
        Post? post = await _postRepository.FindByIdAsync(postId);
        if (post == null)
        {
            throw new KeyNotFoundException($"Post with ID {postId} not found.");
        }

        bool isPostSaved = await _savedPostRepository.IsPostSavedByUserAsync(postId, userId);
        if (isPostSaved)
        {
            throw new InvalidOperationException($"Post with ID {postId} is already saved by user with ID {userId}.");
        }

        var savedPost = new SavedPost
        {
            PostId = postId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _savedPostRepository.AddOrUpdateAsync(savedPost);
    }

    public async Task RemovePostFromSavedPostsAsync(int postId, int userId)
    {
        SavedPost? savedPost = await _savedPostRepository.FindByPostIdAndUserIdAsync(postId, userId);
        if (savedPost == null)
        {
            throw new KeyNotFoundException($"Saved post with PostId {postId} and UserId {userId} not found.");
        }

        await _savedPostRepository.DeleteAsync(savedPost);
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
