namespace TrustTrade.ViewModels;

public class PaginationPartialVM
{
    public int CurrentPage { get; set; }

    public int TotalPages { get; set; }

    public List<int> PagesToShow { get; set; } = new List<int>();

    public string SortOrder { get; set; } = string.Empty;

    public string? CategoryFilter { get; set; } = string.Empty;
}