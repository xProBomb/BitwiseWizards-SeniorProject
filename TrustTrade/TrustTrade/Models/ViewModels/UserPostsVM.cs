namespace TrustTrade.ViewModels;

public class UserPostsVM
{
    public List<PostPreviewVM> Posts { get; set; } = null!;

    public PaginationPartialVM Pagination { get; set; } = null!;

    public PostFiltersPartialVM PostFilters { get; set; } = null!;
}