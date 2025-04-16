using TrustTrade.Models;

namespace TrustTrade.ViewModels;

public class PostPreviewVM
{
    public int Id { get; set; }
    
    public string? UserName { get; set; }

    public string? Title { get; set; }

    public string? Excerpt { get; set; }
    
    public string? TimeAgo { get; set; }
    
    public int LikeCount { get; set; }
    
    public bool IsLikedByCurrentUser { get; set; }

    public int CommentCount { get; set; }

    public bool IsPlaidEnabled { get; set; }

    public string? PortfolioValueAtPosting { get; set; }

    public byte[]? ProfilePicture { get; set; }
}