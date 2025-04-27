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
    /// Creates a new comment for a specific post.
    /// </summary>
    /// <param name="postId">The ID of the post.</param>
    /// <param name="user">The user creating the comment.</param>
    /// <param name="commentDTO">The comment data transfer object.</param>
    /// <returns>The created comment.</returns>
    Task<Comment> CreateCommentAsync(int postId, User user, CommentCreateDTO commentCreateDTO);
}