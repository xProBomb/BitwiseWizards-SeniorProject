using TrustTrade.Models;
using TrustTrade.DAL.Abstract;
using TrustTrade.Services.Web.Interfaces;
using TrustTrade.ViewModels;
using TrustTrade.Helpers;

namespace TrustTrade.Services.Web.Implementations;

/// <summary>
/// Service for comment-related operations.
/// </summary>
public class CommentService : ICommentService
{
    private readonly ILogger<CommentService> _logger;
    private readonly ICommentRepository _commentRepository;

    public CommentService(ILogger<CommentService> logger, ICommentRepository commentRepository)
    {
        _logger = logger;
        _commentRepository = commentRepository;
    }

    public async Task<List<CommentVM>> GetPostCommentsAsync(int postId)
    {
        List<Comment> comments = await _commentRepository.GetCommentsByPostIdAsync(postId);

        return comments.Select(comment =>
        {

            string? portfolioValue = null;

            if (comment.User.PlaidEnabled == true)
            {
                portfolioValue = comment.PortfolioValueAtPosting.HasValue
                    ? FormatCurrencyAbbreviate.FormatCurrencyAbbreviated(comment.PortfolioValueAtPosting.Value)
                    : "$0";
            }

            return new CommentVM
            {
                Id = comment.Id,
                Username = comment.User.Username ?? string.Empty,
                Content = comment.Content,
                TimeAgo = TimeAgoHelper.GetTimeAgo(comment.CreatedAt),
                IsPlaidEnabled = comment.User.PlaidEnabled,
                PortfolioValueAtPosting = portfolioValue,
                ProfilePicture = comment.User.ProfilePicture,
            };
        }).ToList();
    }
}