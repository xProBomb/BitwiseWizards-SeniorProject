using Microsoft.EntityFrameworkCore;
using TrustTrade.DAL.Abstract;
using TrustTrade.Models;

namespace TrustTrade.DAL.Concrete;

/// <summary>
/// Repository for UserBlock entities.
/// </summary>
public class UserBlockRepository : Repository<UserBlock>, IUserBlockRepository
{
    private DbSet<UserBlock> _userBlocks;

    public UserBlockRepository(TrustTradeDbContext context) : base(context)
    {
        _userBlocks = context.UserBlocks;
    }

    public async Task<List<int>> GetBlockedUserIdsAsync(int currentUserId)
    {
        // Fetch the blocked user IDs for the current user.
        return await _userBlocks
            .Where(ub => ub.BlockerId == currentUserId)
            .Select(ub => ub.BlockedId)
            .ToListAsync();
    }
}
