using TrustTrade.Models.DTO;
using TrustTrade.Helpers;

namespace TrustTrade.Models.ExtensionMethods
{
    /// <summary>
    /// Extension methods for the Comment class.
    /// </summary>
    public static class CommentExtensions
    {
        public static CommentResponseDTO ToResponseDTO(this Comment comment)
        {
            string? portfolioValue = null;

            if (comment.User.PlaidEnabled == true)
            {
                portfolioValue = comment.PortfolioValueAtPosting.HasValue
                    ? FormatCurrencyAbbreviate.FormatCurrencyAbbreviated(comment.PortfolioValueAtPosting.Value)
                    : "$0";
            }

            return new CommentResponseDTO
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