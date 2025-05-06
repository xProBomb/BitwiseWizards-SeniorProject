using TrustTrade.Models;

namespace TrustTrade.DAL.Abstract;

/// <summary>
/// Repository interface for CommentLike entities.
/// </summary>
public interface ICommentLikeRepository : IRepository<CommentLike>
{
    /// <summary>
    /// Find a comment like by its comment ID and user ID.
    /// </summary>
    /// <param name="commentId">The ID of the comment.</param>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>The CommentLike entity, or null if not found.</returns>
    Task<CommentLike?> FindByCommentIdAndUserIdAsync(int commentId, int userId);
}