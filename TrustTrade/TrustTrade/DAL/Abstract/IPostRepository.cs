using TrustTrade.Models;

namespace TrustTrade.DAL.Abstract;

public interface IPostRepository : IRepository<Post>
{
    List<Post> GetPagedPosts(int page = 1, int pageSize = 10, string sortOrder = "DateDesc");

    int GetTotalPosts();
}
