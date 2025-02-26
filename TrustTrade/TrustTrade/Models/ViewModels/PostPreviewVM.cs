using TrustTrade.Models;

namespace TrustTrade.ViewModels;

public class PostPreviewVM
{
    public int Id { get; set; }
    
    public string? UserName { get; set; }

    public string? Title { get; set; }

    public string? Excerpt { get; set; }
    
    public string? TimeAgo { get; set; }
    
    
    public bool IsPlaidEnabled { get; set; }
    public decimal? PortfolioValueAtPosting { get; set; }
}