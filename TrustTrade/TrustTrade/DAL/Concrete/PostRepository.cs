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

    public async Task<(List<Post> posts, int totalPosts)> GetPagedPostsAsync(
        string? categoryFilter = null, 
        int pageNumber = 1,
        int pageSize = 10,
        string sortOrder = "DateDesc",
        List<int>? blockedUserIds = null)
    {
        IQueryable<Post> query = _posts
            .Include(p => p.User)
            .Include(p => p.Tags);

        query = ApplyCategoryFilter(query, categoryFilter);
        query = ApplyBlockedUsersFilter(query, blockedUserIds);
        query = ApplySorting(query, sortOrder);

        int totalPosts = await query.CountAsync();
        
        query = ApplyPagination(query, pageNumber, pageSize);
        var posts = await query.ToListAsync();

        return (posts, totalPosts);
    }

    public async Task<(List<Post> posts, int totalPosts)> GetPagedPostsByUserFollowsAsync(
        int currentUserId,
        string? categoryFilter = null,
        int pageNumber = 1,
        int pageSize = 10,
        string sortOrder = "DateDesc",
        List<int>? blockedUserIds = null)
    {
        // Start with a query that includes related entities.
        IQueryable<Post> query = _posts
            .Include(p => p.User)
            .Include(p => p.Tags);

        query = query
            .Where(p => p.User.FollowerFollowerUsers
                .Any(f => f.FollowingUserId == currentUserId));

        query = ApplyCategoryFilter(query, categoryFilter);
        query = ApplyBlockedUsersFilter(query, blockedUserIds);
        query = ApplySorting(query, sortOrder);

        int totalPosts = await query.CountAsync();

        query = ApplyPagination(query, pageNumber, pageSize);
        var posts = await query.ToListAsync();

        return (posts, totalPosts);
    }

    public async Task<(List<Post> posts, int totalPosts)> GetPagedPostsByUserAsync(
        int userId,
        string? categoryFilter = null,
        int pageNumber = 1,
        int pageSize = 10,
        string sortOrder = "DateDesc",
        List<int>? blockedUserIds = null)
    {
        // Start with a query that includes related entities.
        IQueryable<Post> query = _posts
            .Include(p => p.User)
            .Include(p => p.Tags);

        query = query.Where(p => p.UserId == userId);

        query = ApplyCategoryFilter(query, categoryFilter);
        query = ApplyBlockedUsersFilter(query, blockedUserIds);
        query = ApplySorting(query, sortOrder);

        int totalPosts = await query.CountAsync();

        query = ApplyPagination(query, pageNumber, pageSize);
        var posts = await query.ToListAsync();

        return (posts, totalPosts);
    }

    public async Task<(List<Post> posts, int totalPosts)> GetUserSavedPagedPostsAsync(
        int userId,
        string? categoryFilter = null,
        int pageNumber = 1,
        int pageSize = 10,
        string sortOrder = "DateDesc",
        List<int>? blockedUserIds = null)
    {
        // Start with a query that includes related entities.
        IQueryable<Post> query = _posts
            .Include(p => p.User)
            .Include(p => p.Tags);

        query = query.Where(p => p.SavedPosts.Any(sp => sp.UserId == userId));

        query = ApplyCategoryFilter(query, categoryFilter);
        query = ApplyBlockedUsersFilter(query, blockedUserIds);
        query = ApplySorting(query, sortOrder);

        int totalPosts = await query.CountAsync();

        query = ApplyPagination(query, pageNumber, pageSize);
        var posts = await query.ToListAsync();

        return (posts, totalPosts);
    }

    public async Task<List<Post>> SearchPostsAsync(List<string> searchTerms, List<int>? blockedUserIds = null)
    {
        // Start with a query that includes related entities.
        IQueryable<Post> query = _posts
            .Include(p => p.User)
            .Include(p => p.Tags);

        if (searchTerms != null && searchTerms.Count > 0)
        {
            foreach (var term in searchTerms)
            {
                var lowerTerm = term.ToLower();
                query = query.Where(p => p.Title.ToLower().Contains(lowerTerm) || p.Content.ToLower().Contains(lowerTerm));
            }
        }

        query = ApplyBlockedUsersFilter(query, blockedUserIds);

        return await query.ToListAsync();
    }

    private static IQueryable<Post> ApplyBlockedUsersFilter(IQueryable<Post> query, List<int>? blockedUserIds)
    {
        if (blockedUserIds == null || blockedUserIds.Count == 0)
        {
            return query;
        }

        return query.Where(p => !blockedUserIds.Contains(p.UserId));
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

