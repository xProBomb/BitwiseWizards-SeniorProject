namespace TrustTrade.ViewModels;

public class PostDetailsVM
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string TimeAgo { get; set; } = string.Empty;

    public List<string> Tags { get; set; } = new List<string>();
    
    public int LikeCount { get; set; }

    public bool IsLikedByCurrentUser { get; set; }

    public int CommentCount { get; set; }

    public bool IsPlaidEnabled { get; set; }

    public string? PortfolioValueAtPosting { get; set; }

    public bool IsOwnedByCurrentUser { get; set; }

    public bool IsUserAdmin { get; set; }

    public byte[]? ProfilePicture { get; set; }

    public List<CommentVM> Comments { get; set; } = new List<CommentVM>();

    public bool IsSavedByCurrentUser { get; set; }
}