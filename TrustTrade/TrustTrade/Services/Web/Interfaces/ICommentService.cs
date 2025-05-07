using TrustTrade.Models;
using TrustTrade.Models.DTO;
using TrustTrade.ViewModels;

namespace TrustTrade.Services.Web.Interfaces;

/// <summary>
/// Interface for comment-related services.
/// </summary>
public interface ICommentService
{
    /// <summary>
    /// Retrieves comments for a specific post.
    /// </summary>
    /// <param name="postId">The ID of the post.</param>
    /// <returns>A list of comments for the specified post.</returns>
    Task<List<CommentVM>> GetPostCommentsAsync(int postId);

    /// <summary>
    /// Retrieves a comment by its ID.
    /// </summary>
    /// <param name="commentId">The ID of the comment.</param>
    /// <returns>The comment with the specified ID, or null if not found.</returns>
    Task<Comment?> GetCommentByIdAsync(int commentId);

    /// <summary>
    /// Creates a new comment for a specific post.
    /// </summary>
    /// <param name="user">The user creating the comment.</param>
    /// <param name="commentDTO">The comment data transfer object.</param>
    /// <returns>The created comment.</returns>
    Task<Comment> CreateCommentAsync(User user, CommentCreateDTO commentCreateDTO);

    Task<bool> DeleteCommentAsync(int commentId, int userId);

    Task<bool> ToggleCommentLikeAsync(int commentId, int userId);

    Task<int> GetCommentLikeCountAsync(int commentId);
}