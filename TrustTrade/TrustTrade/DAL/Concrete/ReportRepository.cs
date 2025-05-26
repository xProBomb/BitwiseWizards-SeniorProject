using Microsoft.EntityFrameworkCore;
using TrustTrade.DAL.Abstract;
using TrustTrade.Models;

namespace TrustTrade.DAL.Concrete
{
    public class ReportRepository : Repository<Report>, IReportRepository
    {
        private readonly TrustTradeDbContext _context;
        
        public ReportRepository(TrustTradeDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Report> GetReportWithDetailsAsync(int reportId)
        {
            return await _context.Set<Report>()
                .Include(r => r.Reporter)
                .Include(r => r.ReportedUser)
                .Include(r => r.ReportedPost)
                    .ThenInclude(p => p.User)
                .Include(r => r.ReviewedByUser)
                .FirstOrDefaultAsync(r => r.Id == reportId);
        }

        public async Task<List<Report>> GetReportsByUserAsync(int userId)
        {
            return await _context.Set<Report>()
                .Include(r => r.ReportedUser)
                .Include(r => r.ReportedPost)
                .Where(r => r.ReporterId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Report>> GetPendingReportsAsync()
        {
            return await _context.Set<Report>()
                .Include(r => r.Reporter)
                .Include(r => r.ReportedUser)
                .Include(r => r.ReportedPost)
                .Where(r => r.Status == "Pending")
                .OrderBy(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> HasUserReportedEntityAsync(int userId, string reportType, int entityId)
        {
            return await _context.Set<Report>()
                .AnyAsync(r => r.ReporterId == userId && 
                              r.ReportType == reportType &&
                              ((reportType == "Post" && r.ReportedPostId == entityId) ||
                               (reportType == "Profile" && r.ReportedUserId == entityId)));
        }

        public async Task<int> GetReportCountForEntityAsync(string reportType, int entityId)
        {
            return await _context.Set<Report>()
                .CountAsync(r => r.ReportType == reportType &&
                               ((reportType == "Post" && r.ReportedPostId == entityId) ||
                                (reportType == "Profile" && r.ReportedUserId == entityId)));
        }
    }
}