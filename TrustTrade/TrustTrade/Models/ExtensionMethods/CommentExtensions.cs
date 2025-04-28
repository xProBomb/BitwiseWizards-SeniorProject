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
        public static CommentVM ToViewModel(this Comment comment)
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
        }
    }
}