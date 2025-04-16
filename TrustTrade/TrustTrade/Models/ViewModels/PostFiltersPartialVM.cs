namespace TrustTrade.ViewModels;

public class PostFiltersPartialVM
{
    public List<(string Value, string Text)> SortOptions { get; set;} = new List<(string Value, string Text)>();

    public string? SortOrder { get; set; }

    public List<string> Categories { get; set; } = new List<string>();

    public string? SelectedCategory { get; set; }

    public string? SearchQuery { get; set; }
}