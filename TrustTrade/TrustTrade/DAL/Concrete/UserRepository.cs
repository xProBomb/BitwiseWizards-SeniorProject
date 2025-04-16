using Microsoft.EntityFrameworkCore;
using TrustTrade.DAL.Abstract;
using TrustTrade.Models;

namespace TrustTrade.DAL.Concrete;

/// <summary>
/// Repository for User entities.
/// </summary>
public class UserRepository : Repository<User>, IUserRepository
{
    private DbSet<User> _users;

    public UserRepository(TrustTradeDbContext context) : base(context)
    {
        _users = context.Users;
    }

    public async Task<User?> FindByIdentityIdAsync(string identityId, bool includeRelated = false)
    {
        IQueryable<User> users = _users;

        if (includeRelated)
        {
            users = users
                .Include(u => u.Comments)
                .Include(u => u.FollowerFollowerUsers);
        }
        
        return await users.FirstOrDefaultAsync(u => u.IdentityId == identityId);
    }

    public async Task<User?> FindByUsernameAsync(string username, bool includeRelated = false)
    {
        IQueryable<User> users = _users;

        if (includeRelated)
        {
            users = users
                .Include(u => u.Comments)
                .Include(u => u.FollowerFollowerUsers);
        }
        
        return await users.FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
    }
}
