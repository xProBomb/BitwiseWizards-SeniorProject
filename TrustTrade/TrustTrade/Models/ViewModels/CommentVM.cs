namespace TrustTrade.ViewModels;

public class CommentVM
{
    public int Id { get; set; }
    
    public string Username { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;
    
    public string TimeAgo { get; set; } = string.Empty;

    public bool? IsPlaidEnabled { get; set; }

    public string? PortfolioValueAtPosting { get; set; }

    public byte[]? ProfilePicture { get; set; }
}