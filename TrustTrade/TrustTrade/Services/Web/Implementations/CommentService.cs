using TrustTrade.Models;
using TrustTrade.DAL.Abstract;
using TrustTrade.Services.Web.Interfaces;
using TrustTrade.ViewModels;
using TrustTrade.Helpers;
using TrustTrade.Models.DTO;

namespace TrustTrade.Services.Web.Implementations;

/// <summary>
/// Service for comment-related operations.
/// </summary>
public class CommentService : ICommentService
{
    private readonly ILogger<CommentService> _logger;
    private readonly ICommentRepository _commentRepository;
    private readonly IHoldingsRepository _holdingsRepository;
    private readonly INotificationService _notificationService;
    private readonly IPostRepository _postRepository;

    public CommentService(
        ILogger<CommentService> logger, 
        ICommentRepository commentRepository, 
        IHoldingsRepository holdingsRepository,
        INotificationService notificationService,
        IPostRepository postRepository)
    {
        _logger = logger;
        _commentRepository = commentRepository;
        _holdingsRepository = holdingsRepository;
        _notificationService = notificationService;
        _postRepository = postRepository;
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
                IsPlaidEnabled = comment.User.PlaidEnabled ?? false,
                PortfolioValueAtPosting = portfolioValue,
                ProfilePicture = comment.User.ProfilePicture,
            };
        }).ToList();
    }

    public async Task<Comment?> GetCommentByIdAsync(int commentId)
    {
        Comment? comment = await _commentRepository.FindByIdAsync(commentId);
        if (comment == null)
        {
            _logger.LogWarning($"Comment with ID {commentId} not found.");
            return null;
        }

        return comment;
    }

    public async Task<Comment> CreateCommentAsync(User user, CommentCreateDTO commentCreateDTO)
    {
        Post? post = await _postRepository.FindByIdAsync(commentCreateDTO.PostId);
        if (post == null)
        {
            _logger.LogWarning($"Post with ID {commentCreateDTO.PostId} not found.");
            throw new KeyNotFoundException($"Post with ID {commentCreateDTO.PostId} not found.");
        }

        var comment = new Comment
        {
            PostId = commentCreateDTO.PostId,
            UserId = user.Id,
            Content = commentCreateDTO.Content,
            CreatedAt = DateTime.UtcNow
        };

        // Only attempt to refresh and calculate portfolio value if Plaid is enabled
        if (user.PlaidEnabled == true)
        {
            try
            {
                // Refresh the user's holdings using the repository method
                bool refreshSuccess = await _holdingsRepository.RefreshHoldingsAsync(user.Id);

                if (refreshSuccess)
                {
                    // Get the latest holdings after refresh
                    var holdings = await _holdingsRepository.GetHoldingsForUserAsync(user.Id);

                    // Calculate total portfolio value by summing (quantity * current price)
                    decimal totalPortfolioValue = 0;
                    foreach (var holding in holdings)
                    {
                        totalPortfolioValue += holding.Quantity * holding.CurrentPrice;
                    }

                    comment.PortfolioValueAtPosting = totalPortfolioValue;

                    _logger.LogInformation($"User {user.Username} portfolio value at posting: {totalPortfolioValue:C}");
                }
                else
                {
                    // Log warning if refresh fails but continue with post creation
                    _logger.LogWarning($"Failed to refresh holdings for user {user.Username} during comment creation");
                }
            }
            catch (Exception ex)
            {
                // Log error but don't prevent post creation
                _logger.LogError(ex, $"Error calculating portfolio value for user {user.Username}");
            }
        }
        else
        {
            _logger.LogInformation(
                $"User {user.Username} does not have Plaid enabled, skipping portfolio value calculation");
        }

        await _commentRepository.AddOrUpdateAsync(comment);
        await _notificationService.CreateCommentNotificationAsync(user.Id, post.Id ,post.UserId, comment.Id);
        _logger.LogInformation($"Comment created by user {user.Id} on post {commentCreateDTO.PostId}.");

        return comment;
    }

    public async Task<bool> DeleteCommentAsync(int commentId, int userId)
    {
        Comment? comment = await _commentRepository.FindByIdAsync(commentId);
        
        if (comment == null)
        {
            _logger.LogWarning($"Comment with ID {commentId} not found.");
            return false;
        }

        if (comment.UserId != userId)
        {
            _logger.LogWarning($"User {userId} attempted to delete comment {commentId} but does not own it.");
            return false;
        }

        await _commentRepository.DeleteAsync(comment);
        _logger.LogInformation($"Comment with ID {commentId} deleted by user {userId}.");

        return true;
    }

}