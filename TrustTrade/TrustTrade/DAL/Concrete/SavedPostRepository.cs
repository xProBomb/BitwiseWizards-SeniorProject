using Microsoft.EntityFrameworkCore;
using TrustTrade.DAL.Abstract;
using TrustTrade.Models;

namespace TrustTrade.DAL.Concrete;

/// <summary>
/// Repository for Tag entities.
/// </summary>
public class SavedPostRepository : Repository<SavedPost>, ISavedPostRepository
{
    private DbSet<SavedPost> _savedPosts;

    public SavedPostRepository(TrustTradeDbContext context) : base(context)
    {
        _savedPosts = context.SavedPosts;
    }

    public async Task<SavedPost?> FindByPostIdAndUserIdAsync(int postId, int userId)
    {
        return await _savedPosts
            .Include(sp => sp.Post)
            .ThenInclude(p => p.User)
            .FirstOrDefaultAsync(sp => sp.PostId == postId && sp.UserId == userId);
    }

    public async Task<List<SavedPost>> GetSavedPostsByUserIdAsync(int userId)
    {
        return await _savedPosts
            .Include(sp => sp.Post)
            .ThenInclude(p => p.User)
            .Where(sp => sp.UserId == userId)
            .ToListAsync();
    }

    public async Task<bool> IsPostSavedByUserAsync(int postId, int userId)
    {
        return await _savedPosts
            .AnyAsync(sp => sp.PostId == postId && sp.UserId == userId);
    }
}
