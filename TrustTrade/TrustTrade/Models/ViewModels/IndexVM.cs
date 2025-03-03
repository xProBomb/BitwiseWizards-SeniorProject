using TrustTrade.Models;

namespace TrustTrade.ViewModels;

public class IndexVM
{
    public List<PostPreviewVM>? Posts { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public List<int>? PagesToShow { get; set; }
    public string? SortOrder { get; set; }
    public required List<string> Categories { get; set; }
    public string? SelectedCategory { get; set; }
}