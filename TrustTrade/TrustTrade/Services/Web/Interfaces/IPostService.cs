using System.Collections.Generic;
using System.Threading.Tasks;
using TrustTrade.Models;
using TrustTrade.ViewModels;

namespace TrustTrade.Services.Web.Interfaces;

/// <summary>
/// Interface for post-related services.
/// </summary>
public interface IPostService
{
    /// <summary>
    /// Get a list of post previews based on filters and pagination.
    /// </summary>
    /// <param name="categoryFilter">The category filter to apply.</param>
    /// <param name="pageNumber">The page number for pagination.</param>
    /// <param name="sortOrder">The sort order for the posts.</param>
    /// <returns>A list of post previews.</returns>
    Task<(List<Post> posts, int totalPosts)> GetPagedPostsAsync(string? categoryFilter, int pageNumber, string sortOrder, int? currentUserId);

    /// <summary>
    /// Get a list of post previews for posts from users that the current user follows.
    /// </summary>
    /// <param name="currentUserId">The ID of the current user.</param>
    /// <param name="categoryFilter">The category filter to apply.</param>
    /// <param name="pageNumber">The page number for pagination.</param>
    /// <param name="sortOrder">The sort order for the posts.</param>
    /// <returns>A list of post previews.</returns>
    Task<(List<Post> posts, int totalPosts)> GetFollowingPagedPostsAsync(int currentUserId, string? categoryFilter, int pageNumber, string sortOrder);

    /// <summary>
    /// Get a list of post previews for posts from a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user whose posts to retrieve.</param>
    /// <param name="categoryFilter">The category filter to apply.</param>
    /// <param name="pageNumber">The page number for pagination.</param>
    /// <param name="sortOrder">The sort order for the posts.</param>
    /// <returns>A list of post previews.</returns>
    Task<(List<Post> posts, int totalPosts)> GetUserPagedPostsAsync(int userId, string? categoryFilter, int pageNumber, string sortOrder);

    /// <summary>
    /// Search for posts based on a list of search terms.
    /// </summary>
    /// <param name="searchTerms">The search terms to filter posts.</param>
    /// <param name="categoryFilter">The category filter to apply.</param>
    /// <param name="pageNumber">The page number for pagination.</param>
    /// <param name="sortOrder">The sort order for the posts.</param>
    /// <returns>A list of post previews.</returns>
    Task<List<Post>> SearchPostsAsync(List<string> searchTerms, int? currentUserId);

    /// <summary>
    /// Build the post filters for the view model.
    /// </summary>
    /// <param name="categoryFilter">The selected category filter.</param>
    /// <param name="sortOrder">The selected sort order.</param>
    /// <param name="searchQuery">The search query string.</param>
    /// <returns>A PostFiltersPartialVM object containing filter information.</returns>
    Task<PostFiltersPartialVM> BuildPostFiltersAsync(string? categoryFilter, string sortOrder, string? searchQuery = null);

    /// <summary>
    /// Build the pagination for the view model.
    /// </summary>
    /// <param name="categoryFilter">The category filter to apply.</param>
    /// <param name="pageNumber">The page number for pagination.</param>
    /// <returns>A PaginationPartialVM object containing pagination information.</returns>
    Task<PaginationPartialVM> BuildPaginationAsync(string? categoryFilter, int pageNumber, int totalPosts, int? currentUserId);

    /// <summary>
    /// Build the pagination for the view model based on search terms.
    /// </summary>
    /// <param name="search">The search string to filter posts.</param>
    /// <param name="searchTerms">The search terms to filter posts.</param>
    /// <param name="categoryFilter">The category filter to apply.</param>
    /// <param name="pageNumber">The page number for pagination.</param>
    /// <returns>A PaginationPartialVM object containing pagination information.</returns>
    Task<PaginationPartialVM> BuildSearchPaginationAsync(string search, List<string> searchTerms, string? categoryFilter, int pageNumber);

    Task AddPostToSavedPostsAsync(int postId, int userId);

    Task RemovePostFromSavedPostsAsync(int postId, int userId);

    Task<(List<Post> posts, int totalPosts)> GetUserSavedPagedPostsAsync(int userId, string? categoryFilter, int pageNumber, string sortOrder);
}