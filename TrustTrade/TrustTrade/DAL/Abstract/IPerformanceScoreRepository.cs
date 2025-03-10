using TrustTrade.Models;

namespace TrustTrade.DAL.Abstract
{
    /// <summary>
    /// Repository for calculating user performance scores based on portfolio metrics
    /// </summary>
    public interface IPerformanceScoreRepository
    {
        /// <summary>
        /// Calculates a comprehensive performance score for a user
        /// </summary>
        /// <param name="userId">The user's ID</param>
        /// <returns>Tuple containing the score, whether it's rated, and a breakdown of components</returns>
        Task<(decimal Score, bool IsRated, Dictionary<string, decimal> Breakdown)> CalculatePerformanceScoreAsync(int userId);
        
        /// <summary>
        /// Determines if a user meets minimum requirements for performance scoring
        /// </summary>
        /// <param name="user">The user entity to evaluate</param>
        /// <returns>True if the user meets requirements, false otherwise</returns>
        bool MeetsMinimumRequirements(User user);
    }
}