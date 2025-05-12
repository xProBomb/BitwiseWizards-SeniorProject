using Microsoft.EntityFrameworkCore;
using TrustTrade.DAL.Abstract;
using TrustTrade.Data;
using TrustTrade.Models;

namespace TrustTrade.DAL.Concrete
{
    public class LikeRepository : Repository<Like>, ILikeRepository
    {
        private readonly DbSet<Like> _likes;

        public LikeRepository(TrustTradeDbContext context) : base(context)
        {
            _likes = context.Likes;
        }

        public async Task<Like?> GetLikeByUserAndPostAsync(int userId, int postId)
        {
            return await _likes
                .FirstOrDefaultAsync(l => l.UserId == userId && l.PostId == postId);
        }
    }
}
