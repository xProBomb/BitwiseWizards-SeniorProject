using TrustTrade.Models.DTO;
using TrustTrade.Helpers;
using TrustTrade.ViewModels;

namespace TrustTrade.Models.ExtensionMethods
{
    /// <summary>
    /// Extension methods for the Comment class.
    /// </summary>
    public static class CommentExtensions
    {
        public static CommentVM ToViewModel(this Comment comment, User? user = null)
        {
            string? commentPortfolioValue = null;

            if (comment.User?.PlaidEnabled == true)
            {
                commentPortfolioValue = comment.PortfolioValueAtPosting.HasValue
                    ? FormatCurrencyAbbreviate.FormatCurrencyAbbreviated(comment.PortfolioValueAtPosting.Value)
                    : "$0";
            }

            return new CommentVM
            {
                Id = comment.Id,
                Username = comment.User?.Username ?? "Unknown",
                Content = comment.Content,
                TimeAgo = TimeAgoHelper.GetTimeAgo(comment.CreatedAt),
                IsPlaidEnabled = comment.User?.PlaidEnabled ?? false,
                PortfolioValueAtPosting = commentPortfolioValue,
                ProfilePicture = comment.User?.Is_Suspended == true ? Array.Empty<byte>(): comment.User?.ProfilePicture,
                IsOwnedByCurrentUser = user != null && comment.UserId == user.Id,
                LikeCount = comment.CommentLikes?.Count ?? 0,
                IsLikedByCurrentUser = user != null && comment.CommentLikes?.Any(l => l.UserId == user.Id) == true,
            };
        }
    }
}