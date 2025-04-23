using TrustTrade.Models;

namespace TrustTrade.DAL.Abstract;

/// <summary>
/// Repository interface for Comment entities.
/// </summary>
public interface ICommentRepository : IRepository<Comment>
{
    /// <summary>
    /// Get all comments for a specific post.
    /// </summary>
    /// <param name="postId">The ID of the post.</param>
    /// <returns>A list of comments for the specified post.</returns>
    Task<List<Comment>> GetCommentsByPostIdAsync(int postId);
}