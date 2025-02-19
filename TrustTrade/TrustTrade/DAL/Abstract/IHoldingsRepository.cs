using TrustTrade.Models;

namespace TrustTrade.DAL.Abstract;

/// <summary>
/// Repository interface for managing investment holdings
/// </summary>
public interface IHoldingsRepository : IRepository<InvestmentPosition>
{
    /// <summary>
    /// Gets all holdings for a specific user
    /// </summary>
    /// <param name="userId">The user's ID</param>
    /// <returns>List of investment positions</returns>
    Task<List<InvestmentPosition>> GetHoldingsForUserAsync(int userId);

    /// <summary>
    /// Refreshes holdings data from Plaid for a user
    /// </summary>
    /// <param name="userId">The user's ID</param>
    /// <returns>True if refresh was successful, false otherwise</returns>
    Task<bool> RefreshHoldingsAsync(int userId);

    /// <summary>
    /// Removes all holdings for a specific user
    /// </summary>
    /// <param name="userId">The user's ID</param>
    /// <returns>Number of holdings removed</returns>
    Task<int> RemoveHoldingsForUserAsync(int userId);
}