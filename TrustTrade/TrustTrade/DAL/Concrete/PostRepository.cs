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

    public async Task<List<Post>> GetPagedPostsAsync(string? category = null, int page = 1, int pageSize = 10, string sortOrder = "DateDesc")
    {
        // Start with a query that includes the related User and Tags.
        IQueryable<Post> query = _posts
            .Include(p => p.User)
            .Include(p => p.Tags);

        // Apply filtering based on the category parameter
        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(p => p.Tags.Any(t => t.TagName.ToLower() == category.ToLower()));
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
        return await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    // Method to get the posts that a specific user is following
    public async Task<List<Post>> GetPagedPostsByUserFollowsAsync(int currentUser, string? category = null, int page = 1, int pageSize = 10, string sortOrder = "DateDesc")
    {
        // Start with a query that includes the related User and Tags.
        IQueryable<Post> query = _posts
            .Include(p => p.User)
            .Include(p => p.Tags)
            .Where(p => p.User.FollowerFollowerUsers.Any(f => f.FollowingUserId == currentUser));

        // Apply filtering based on the category parameter
        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(p => p.Tags.Any(t => t.TagName.ToLower() == category.ToLower()));
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
        return await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetTotalPostsAsync(string? category = null)
    {
        // Start with a query that includes the related Tags.
        IQueryable<Post> query = _posts
            .Include(p => p.Tags);

        // Apply filtering based on the category parameter
        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(p => p.Tags.Any(t => t.TagName.ToLower() == category.ToLower()));
        }

        return await query.CountAsync();
    }

    public async Task<int> GetTotalPostsByUserFollowsAsync(int currentUserId, string? category = null)
    {
        // Start with a query that includes the related Tags.
        IQueryable<Post> query = _posts
            .Include(p => p.Tags)
            .Where(p => p.User.FollowerFollowerUsers.Any(f => f.FollowingUserId == currentUserId));

        // Apply filtering based on the category parameter
        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(p => p.Tags.Any(t => t.TagName.ToLower() == category.ToLower()));
        }

        return await query.CountAsync();
    }
}

