using TrustTrade.Models;

namespace TrustTrade.DAL.Abstract;

public interface IPostRepository : IRepository<Post>
{
    Task<List<Post>> GetPagedPostsAsync(string? category = null, int page = 1, int pageSize = 10, string sortOrder = "DateDesc");
    Task<List<Post>> GetPagedPostsByUserFollowsAsync(int currentUserId, string? categoryFilter, int page, int pAGE_SIZE, string sortOrder);
    Task<int> GetTotalPostsAsync(string? category = null);
    Task<int> GetTotalPostsByUserFollowsAsync(int currentUserId, string? categoryFilter);
}
