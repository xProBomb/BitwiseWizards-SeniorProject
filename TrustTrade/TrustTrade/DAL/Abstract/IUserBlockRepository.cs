using TrustTrade.Models;

namespace TrustTrade.DAL.Abstract;

/// <summary>
/// Repository interface for UserBlock entities.
/// </summary>
public interface IUserBlockRepository : IRepository<UserBlock>
{
    Task<List<int>> GetBlockedUserIdsAsync(int currentUserId);
}