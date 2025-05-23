namespace TrustTrade.ViewModels;

public class UserSavesVM
{
    public List<PostPreviewVM> Posts { get; set; } = null!;

    public PaginationPartialVM Pagination { get; set; } = null!;

    public PostFiltersPartialVM PostFilters { get; set; } = null!;

    public string Username { get; set; } = null!;
}