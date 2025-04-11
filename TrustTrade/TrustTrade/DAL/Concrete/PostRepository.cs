using System.Linq;
using System.Threading.Tasks;
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

    public async Task<List<Post>> GetPagedPostsAsync(string? categoryFilter = null, int pageNumber = 1, int pageSize = 10, string sortOrder = "DateDesc")
    {
        // Start with a query that includes related entities.
        IQueryable<Post> query = _posts
            .Include(p => p.User)
            .Include(p => p.Tags);

        query = ApplyCategoryFilter(query, categoryFilter);
        query = ApplySorting(query, sortOrder);
        query = ApplyPagination(query, pageNumber, pageSize);

        return await query.ToListAsync();
    }

    public async Task<List<Post>> GetPagedPostsByUserFollowsAsync(int currentUserId, string? categoryFilter = null, int pageNumber = 1, int pageSize = 10, string sortOrder = "DateDesc")
    {
        // Start with a query that includes related entities.
        IQueryable<Post> query = _posts
            .Include(p => p.User)
            .Include(p => p.Tags);

        query = ApplyUserFollowsFilter(query, currentUserId);
        query = ApplyCategoryFilter(query, categoryFilter);
        query = ApplySorting(query, sortOrder);
        query = ApplyPagination(query, pageNumber, pageSize);

        return await query.ToListAsync();
    }

    public async Task<int> GetTotalPostsAsync(string? categoryFilter = null)
    {
        // Start with a query that includes the related Tags.
        IQueryable<Post> query = _posts
            .Include(p => p.Tags);

        query = ApplyCategoryFilter(query, categoryFilter);

        return await query.CountAsync();
    }

    public async Task<int> GetTotalPostsByUserFollowsAsync(int currentUserId, string? categoryFilter = null)
    {
        // Start with a query that includes the related Tags.
        IQueryable<Post> query = _posts
            .Include(p => p.Tags);

        query = ApplyUserFollowsFilter(query, currentUserId);
        query = ApplyCategoryFilter(query, categoryFilter);

        return await query.CountAsync();
    }

    private static IQueryable<Post> ApplyUserFollowsFilter(IQueryable<Post> query, int currentUserId)
    {
        return query.Where(p => p.User.FollowerFollowerUsers.Any(f => f.FollowingUserId == currentUserId));
    }

    private static IQueryable<Post> ApplyCategoryFilter(IQueryable<Post> query, string? category)
    {
        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(p => p.Tags.Any(t => t.TagName.ToLower() == category.ToLower()));
        }
        return query;
    }

    private static IQueryable<Post> ApplySorting(IQueryable<Post> query, string sortOrder)
    {
        switch (sortOrder)
        {
            case "DateAsc":
                return query.OrderBy(p => p.CreatedAt);
            case "TitleAsc":
                return query.OrderBy(p => p.Title);
            case "TitleDesc":
                return query.OrderByDescending(p => p.Title);
            case "DateDesc":
            default:
                return query.OrderByDescending(p => p.CreatedAt);
        }
    }

    private static IQueryable<Post> ApplyPagination(IQueryable<Post> query, int pageNumber, int pageSize)
    {
        return query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
    }


}

