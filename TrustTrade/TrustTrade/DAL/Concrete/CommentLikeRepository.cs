using Microsoft.EntityFrameworkCore;
using TrustTrade.DAL.Abstract;
using TrustTrade.Models;

namespace TrustTrade.DAL.Concrete;

/// <summary>
/// Repository for CommentLike entities.
/// </summary>
public class CommentLikeRepository : Repository<CommentLike>, ICommentLikeRepository
{
    private DbSet<CommentLike> _commentLikes;

    public CommentLikeRepository(TrustTradeDbContext context) : base(context)
    {
        _commentLikes = context.CommentLikes;
    }

    public async Task<CommentLike?> FindByCommentIdAndUserIdAsync(int commentId, int userId)
    {
        return await _commentLikes
            .FirstOrDefaultAsync(cl => cl.CommentId == commentId && cl.UserId == userId);
    }

    public async Task<int> GetLikeCountByCommentIdAsync(int commentId)
    {
        return await _commentLikes
            .CountAsync(cl => cl.CommentId == commentId);
    }
}
