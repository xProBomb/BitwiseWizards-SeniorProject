using TrustTrade.Models;

namespace TrustTrade.DAL.Abstract
{
    public interface IReportRepository : IRepository<Report>
    {
        Task<Report> GetReportWithDetailsAsync(int reportId);
        Task<List<Report>> GetReportsByUserAsync(int userId);
        Task<List<Report>> GetPendingReportsAsync();
        Task<bool> HasUserReportedEntityAsync(int userId, string reportType, int entityId);
        Task<int> GetReportCountForEntityAsync(string reportType, int entityId);
    }
}