using Microsoft.EntityFrameworkCore;
using TrustTrade.DAL.Abstract;
using TrustTrade.Data;
using TrustTrade.Models;

namespace TrustTrade.DAL.Concrete
{
    public class LikeRepository : Repository<Like>, ILikeRepository
    {
        private readonly TrustTradeDbContext _context;
        private readonly DbSet<Like> _dbSet;

        public LikeRepository(TrustTradeDbContext context) : base(context)
        {
            _context = context;
            _dbSet = context.Set<Like>();
        }

        public async Task<Like?> GetLikeByUserAndPostAsync(int userId, int postId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(l => l.UserId == userId && l.PostId == postId);
        }
    }
}
