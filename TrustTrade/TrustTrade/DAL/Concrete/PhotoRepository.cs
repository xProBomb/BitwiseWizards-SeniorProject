using Microsoft.EntityFrameworkCore;
using TrustTrade.DAL.Abstract;
using TrustTrade.Data;
using TrustTrade.Models;

namespace TrustTrade.DAL.Concrete
{
    public class PhotoRepository : Repository<Photo>, IPhotoRepository
    {
        private readonly TrustTradeDbContext _context;
        private readonly ILogger<PhotoRepository> _logger;

        public PhotoRepository(TrustTradeDbContext context, ILogger<PhotoRepository> logger) 
            : base(context)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Photo?> FindByIdAsync(int id)
        {
            return await _context.Photos.FindAsync(id);
        }

        public async Task<IEnumerable<Photo>> GetPhotosByPostIdAsync(int postId)
        {
            return await _context.Photos
                .Where(p => p.PostId == postId)
                .OrderBy(p => p.CreatedAt)
                .ToListAsync();
        }
    }
}