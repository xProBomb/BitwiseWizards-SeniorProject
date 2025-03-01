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

    public List<Post> GetPagedPosts(string? category, int page = 1, int pageSize = 10, string sortOrder = "DateDesc")
{
    // Start with a query that includes the related User and Tags.
    IQueryable<Post> query = _posts
        .Include(p => p.User)
        .Include(p => p.Tags);

    // Apply filtering based on the category parameter
    if (!string.IsNullOrEmpty(category))
    {
        query = query.Where(p => p.Tags.Any(t => t.TagName.Equals(category, StringComparison.OrdinalIgnoreCase)));
    }

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

