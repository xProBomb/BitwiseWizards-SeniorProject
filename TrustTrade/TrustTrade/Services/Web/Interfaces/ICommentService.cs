using System.Collections.Generic;
using System.Threading.Tasks;
using TrustTrade.Models;
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
}