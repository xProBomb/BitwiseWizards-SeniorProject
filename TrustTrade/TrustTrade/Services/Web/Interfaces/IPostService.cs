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
    Task<List<PostPreviewVM>> GetPostPreviewsAsync(string? categoryFilter, int pageNumber, string sortOrder);

    /// <summary>
    /// Get a list of post previews for posts from users that the current user follows.
    /// </summary>
    /// <param name="currentUser">The ID of the current user.</param>
    /// <param name="categoryFilter">The category filter to apply.</param>
    /// <param name="pageNumber">The page number for pagination.</param>
    /// <param name="sortOrder">The sort order for the posts.</param>
    /// <returns>A list of post previews.</returns>
    Task<List<PostPreviewVM>> GetFollowingPostPreviewsAsync(int currentUser, string? categoryFilter, int pageNumber, string sortOrder);

    /// <summary>
    /// Build the post filters for the view model.
    /// </summary>
    /// <param name="categoryFilter">The selected category filter.</param>
    /// <param name="sortOrder">The selected sort order.</param>
    /// <returns>A PostFiltersPartialVM object containing filter information.</returns>
    Task<PostFiltersPartialVM> BuildPostFiltersAsync(string? categoryFilter, string sortOrder);

    /// <summary>
    /// Build the pagination for the view model.
    /// </summary>
    /// <param name="categoryFilter">The category filter to apply.</param>
    /// <param name="pageNumber">The page number for pagination.</param>
    /// <returns>A PaginationPartialVM object containing pagination information.</returns>
    Task<PaginationPartialVM> BuildPaginationAsync(string? categoryFilter, int pageNumber);

}