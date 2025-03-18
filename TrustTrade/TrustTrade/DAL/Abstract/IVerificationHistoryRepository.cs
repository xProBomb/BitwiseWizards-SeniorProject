using TrustTrade.Models;

namespace TrustTrade.DAL.Abstract
{
    /// <summary>
    /// Repository interface for managing verification history records
    /// </summary>
    public interface IVerificationHistoryRepository : IRepository<VerificationHistory>
    {
        /// <summary>
        /// Gets the verification history for a specific user
        /// </summary>
        /// <param name="userId">The user's ID</param>
        /// <returns>List of verification history records</returns>
        Task<List<VerificationHistory>> GetHistoryForUserAsync(int userId);

        /// <summary>
        /// Gets the most recent verification status for a user
        /// </summary>
        /// <param name="userId">The user's ID</param>
        /// <returns>The most recent verification history record, or null if none exists</returns>
        Task<VerificationHistory?> GetMostRecentStatusAsync(int userId);

        /// <summary>
        /// Adds a new verification status record
        /// </summary>
        /// <param name="userId">The user's ID</param>
        /// <param name="isVerified">The verification status</param>
        /// <param name="reason">Optional reason for the status change</param>
        /// <param name="source">Optional source of the verification</param>
        /// <returns>The created verification history record</returns>
        Task<VerificationHistory> AddVerificationRecordAsync(int userId, bool isVerified, string? reason = null, string? source = null);

        /// <summary>
        /// Calculates the total duration a user has been verified
        /// </summary>
        /// <param name="userId">The user's ID</param>
        /// <returns>TimeSpan representing the total verified duration</returns>
        Task<TimeSpan> CalculateVerifiedDurationAsync(int userId);

        /// <summary>
        /// Gets the first and most recent verification dates for a user
        /// </summary>
        /// <param name="userId">The user's ID</param>
        /// <returns>Tuple with first verified date and most recent verified date (null if never verified)</returns>
        Task<(DateTime? FirstVerified, DateTime? MostRecentVerified)> GetVerificationDatesAsync(int userId);
    }
}