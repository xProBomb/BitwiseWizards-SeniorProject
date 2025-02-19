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

   public List<Post> GetPagedPosts(int page = 1, int pageSize = 10, string sortOrder = "DateDesc")
{
    // Start with your existing query, including the related User and defaulting if empty.
    var query = _posts
        .Include(p => p.User)
        .DefaultIfEmpty();

    // Apply sorting based on the sortOrder parameter while keeping your original structure
    switch (sortOrder)
    {
        case "DateAsc":
            query = query.OrderBy(p => p.CreatedAt);
            break;
        case "TitleAsc":
            query = query.OrderBy(p => p.Title);
            break;
        case "TitleDesc":
            query = query.OrderByDescending(p => p.Title);
            break;
        case "DateDesc":
        default:
            query = query.OrderByDescending(p => p.CreatedAt);
            break;
    }

    // Apply pagination
    return query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToList();
}

public int GetTotalPosts()
{
    return _posts.Count();
}
}

