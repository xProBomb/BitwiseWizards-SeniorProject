using System.Linq;
using Microsoft.EntityFrameworkCore;
using TrustTrade.DAL.Abstract;
using TrustTrade.Models;

namespace TrustTrade.DAL.Concrete;

public class PostRepository : Repository<Post>, IPostRepository
{
    private DbSet<Post> _posts;

    public PostRepository(TrustTradeDbContext context) : base(context)
    {
        _posts = context.Posts;
    }

    public List<Post> GetPagedPosts(int page = 1, int pageSize = 10)
    {
        return _posts
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }
}