using TrustTrade.Models;

namespace TrustTrade.DAL.Abstract;

/// <summary>
/// Repository interface for SavedPost entities.
/// </summary>
public interface ISavedPostRepository : IRepository<SavedPost>
{
    Task<SavedPost?> FindByPostIdAndUserIdAsync(int postId, int userId);

    /// <summary>
    /// Get all saved posts for a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>A list of saved posts for the specified user.</returns>
    Task<List<SavedPost>> GetSavedPostsByUserIdAsync(int userId);

    /// <summary>
    /// Check if a post is saved by a specific user.
    /// </summary>
    /// <param name="postId">The ID of the post.</param>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>True if the post is saved by the user, otherwise false.</returns>
    Task<bool> IsPostSavedByUserAsync(int postId, int userId);
}