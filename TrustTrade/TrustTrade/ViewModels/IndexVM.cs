using TrustTrade.Models;

namespace TrustTrade.ViewModels;

public class IndexVM
{
    public List<PostPreviewVM>? Posts { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
}