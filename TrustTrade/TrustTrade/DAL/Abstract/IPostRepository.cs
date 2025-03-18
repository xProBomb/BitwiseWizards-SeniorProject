using TrustTrade.Models;

namespace TrustTrade.DAL.Abstract;

public interface IPostRepository : IRepository<Post>
{
    List<Post> GetPagedPosts(string? category = null, int page = 1, int pageSize = 10, string sortOrder = "DateDesc");
    List<Post> GetPagedPostsByUserFollows(int currentUserId, string? categoryFilter, int page, int pAGE_SIZE, string sortOrder);
    int GetTotalPosts(string? category = null);
    int GetTotalPostsByUserFollows(int currentUserId, string? categoryFilter);
}
