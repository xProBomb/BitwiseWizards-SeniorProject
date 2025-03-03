using TrustTrade.Models;

namespace TrustTrade.DAL.Abstract;

public interface IPostRepository : IRepository<Post>
{
    List<Post> GetPagedPosts(string? category = null, int page = 1, int pageSize = 10, string sortOrder = "DateDesc");

    int GetTotalPosts(string? category = null);
}
