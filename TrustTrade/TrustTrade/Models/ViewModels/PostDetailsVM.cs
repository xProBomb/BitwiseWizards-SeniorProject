using Microsoft.Identity.Client;

namespace TrustTrade.ViewModels;

public class PostDetailsVM
{
    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string TimeAgo { get; set; } = string.Empty;

    public List<string> Tags { get; set; } = new List<string>();
    
    public int LikeCount { get; set; }

    public int CommentCount { get; set; }

    public bool IsPlaidEnabled { get; set; }

    public string? PortfolioValueAtPosting { get; set; }
}