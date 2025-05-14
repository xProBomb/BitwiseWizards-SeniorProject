using TrustTrade.Models;

namespace TrustTrade.DAL.Abstract;

/// <summary>
/// Repository interface for Post entities.
/// </summary>
public interface IPostRepository : IRepository<Post>
{
    /// <summary>
    /// Get a paginated list of posts, optionally filtered by category and sorted by specified order.
    /// </summary>
    /// <param name="categoryFilter">Optional category filter.</param>
    /// <param name="pageNumber">Page number for pagination.</param>
    /// <param name="pageSize">Number of posts per page.</param>
    /// <param name="sortOrder">Sorting order (e.g., "DateDesc", "DateAsc").</param>
    /// <returns>A list of posts.</returns>
    Task<(List<Post> posts, int totalPosts)> GetPagedPostsAsync(string? categoryFilter = null, int pageNumber = 1, int pageSize = 10, string sortOrder = "DateDesc", List<int>? blockedUserIds = null);

    /// <summary>
    /// Get a paginated list of posts that the specified user is following, optionally filtered by category and sorted by specified order.
    /// </summary>
    /// <param name="currentUserId">The ID of the current user.</param>
    /// <param name="categoryFilter">Optional category filter.</param>
    /// <param name="pageNumber">Page number for pagination.</param>
    /// <param name="pageSize">Number of posts per page.</param>
    /// <param name="sortOrder">Sorting order (e.g., "DateDesc", "DateAsc").</param>
    /// <returns>A list of posts that the user is following.</returns>
    Task<(List<Post> posts, int totalPosts)> GetPagedPostsByUserFollowsAsync(int currentUserId, string? categoryFilter = null, int pageNumber = 1, int pageSize = 10, string sortOrder = "DateDesc", List<int>? blockedUserIds = null);

    /// <summary>
    /// Get a paginated list of posts by a specific user, optionally filtered by category and sorted by specified order.
    /// </summary>
    /// <param name="userId">The ID of the user whose posts to retrieve.</param>
    /// <param name="categoryFilter">Optional category filter.</param>
    /// <param name="pageNumber">Page number for pagination.</param>
    /// <param name="pageSize">Number of posts per page.</param>
    /// <param name="sortOrder">Sorting order (e.g., "DateDesc", "DateAsc").</param>
    /// <returns>A list of posts by the specified user.</returns>
    Task<(List<Post> posts, int totalPosts)> GetPagedPostsByUserAsync(int userId, string? categoryFilter = null, int pageNumber = 1, int pageSize = 10, string sortOrder = "DateDesc", List<int>? blockedUserIds = null);

    /// <summary>
    /// Search for posts based on a list of search terms, optionally filtered by category and sorted by specified order.
    /// </summary>
    /// <param name="searchTerms">List of search terms to filter posts.</param>
    /// <param name="categoryFilter">Optional category filter.</param>
    /// <param name="pageNumber">Page number for pagination.</param>
    /// <param name="pageSize">Number of posts per page.</param>
    /// <param name="sortOrder">Sorting order (e.g., "DateDesc", "DateAsc").</param>
    /// <returns>A list of posts that match the search criteria.</returns>
    Task<List<Post>> SearchPostsAsync(List<string> searchTerms, List<int>? blockedUserIds = null);

    Task<(List<Post> posts, int totalPosts)> GetUserSavedPagedPostsAsync(int userId, string? categoryFilter = null, int pageNumber = 1, int pageSize = 10, string sortOrder = "DateDesc", List<int>? blockedUserIds = null);
}
