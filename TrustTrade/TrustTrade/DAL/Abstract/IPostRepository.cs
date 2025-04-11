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
    Task<List<Post>> GetPagedPostsAsync(string? categoryFilter = null, int pageNumber = 1, int pageSize = 10, string sortOrder = "DateDesc");

    /// <summary>
    /// Get a paginated list of posts that the specified user is following, optionally filtered by category and sorted by specified order.
    /// </summary>
    /// <param name="currentUserId">The ID of the current user.</param>
    /// <param name="categoryFilter">Optional category filter.</param>
    /// <param name="pageNumber">Page number for pagination.</param>
    /// <param name="pageSize">Number of posts per page.</param>
    /// <param name="sortOrder">Sorting order (e.g., "DateDesc", "DateAsc").</param>
    /// <returns>A list of posts that the user is following.</returns>
    Task<List<Post>> GetPagedPostsByUserFollowsAsync(int currentUserId, string? categoryFilter, int pageNumber, int pageSize, string sortOrder);

    /// <summary>
    /// Get the total number of posts, optionally filtered by category.
    /// </summary>
    /// <param name="categoryFilter">Optional category filter.</param>
    /// <returns>The total number of posts.</returns>
    Task<int> GetTotalPostsAsync(string? categoryFilter = null);

    /// <summary>
    /// Get the total number of posts that the specified user is following, optionally filtered by category.
    /// </summary>
    /// <param name="currentUserId">The ID of the current user.</param>
    /// <param name="categoryFilter">Optional category filter.</param>
    /// <returns>The total number of posts that the user is following.</returns>
    Task<int> GetTotalPostsByUserFollowsAsync(int currentUserId, string? categoryFilter);
}
