using TrustTrade.Models.DTO;
using TrustTrade.Helpers;
using TrustTrade.ViewModels;

namespace TrustTrade.Models.ExtensionMethods
{
    /// <summary>
    /// Extension methods for the Post class.
    /// </summary>
    public static class PostExtensions
    {
        public static PostPreviewVM ToPreviewViewModel(this Post post, int? currentUserId = null)
        {
            var isPlaidEnabled = post.User.PlaidEnabled ?? false;
            string? portfolioValue = null;

            // Retreive and format the portfolio value if Plaid is enabled
            if (isPlaidEnabled)
            {
                if (post.PortfolioValueAtPosting.HasValue)
                {
                    portfolioValue = FormatCurrencyAbbreviate.FormatCurrencyAbbreviated(post.PortfolioValueAtPosting.Value);
                }
                else
                {
                    portfolioValue = "$0";
                }
            }

            return new PostPreviewVM
            {
                Id = post.Id,
                UserName = post.User.Username,
                Title = post.Title,
                Excerpt = post.Content != null && post.Content.Length > 100 
                    ? $"{post.Content.Substring(0, 100)}..." 
                    : post.Content ?? string.Empty,
                TimeAgo = TimeAgoHelper.GetTimeAgo(post.CreatedAt),
                LikeCount = post.Likes.Count,
                CommentCount = post.Comments.Count,
                IsPlaidEnabled = post.User.PlaidEnabled ?? false,
                PortfolioValueAtPosting = portfolioValue,
                ProfilePicture = post.User.ProfilePicture,
                IsSavedByCurrentUser = post.SavedPosts.Any(sp => sp.UserId == currentUserId),
            };
        }

        public static List<PostPreviewVM> ToPreviewViewModels(this List<Post> posts, int? currentUserId = null)
        {
            return posts.Select(post => post.ToPreviewViewModel(currentUserId)).ToList();
        }
    }
}