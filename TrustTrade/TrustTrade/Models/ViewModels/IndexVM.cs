namespace TrustTrade.ViewModels;

public class IndexVM
{
    public List<PostPreviewVM>? Posts { get; set; }

    public PaginationPartialVM? Pagination { get; set; }

    public PostFiltersPartialVM? PostFilters { get; set; }
}