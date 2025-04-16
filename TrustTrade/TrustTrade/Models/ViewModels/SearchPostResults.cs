namespace TrustTrade.ViewModels;

public class SearchPostResultsVM
{
    public List<PostPreviewVM>? Posts { get; set; }

    public PaginationPartialVM? Pagination { get; set; }

    public PostFiltersPartialVM? PostFilters { get; set; }

    public string SearchQuery { get; set; } = string.Empty;
}