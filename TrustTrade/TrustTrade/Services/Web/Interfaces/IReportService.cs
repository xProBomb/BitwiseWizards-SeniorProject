using TrustTrade.Models;

namespace TrustTrade.Services.Web.Interfaces
{
    public interface IReportService
    {
        Task<Report> CreateReportAsync(int reporterId, string reportType, int? postId, int? userId, string category, string description);
        Task<bool> HasUserAlreadyReportedAsync(int userId, string reportType, int entityId);
        Task SendReportNotificationsAsync(Report report);
    }
}