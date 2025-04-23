using Microsoft.EntityFrameworkCore;
using TrustTrade.DAL.Abstract;
using TrustTrade.Models;

namespace TrustTrade.DAL.Concrete;

/// <summary>
/// Repository for Comment entities.
/// </summary>
public class CommentRepository : Repository<Comment>, ICommentRepository
{
    private DbSet<Comment> _comments;

    public CommentRepository(TrustTradeDbContext context) : base(context)
    {
        _comments = context.Comments;
    }

    public async Task<List<Comment>> GetCommentsByPostIdAsync(int postId)
    {
        return await _comments
            .Include(c => c.User)
            .Where(c => c.PostId == postId)
            .ToListAsync();
    }
}
