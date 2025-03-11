using Microsoft.EntityFrameworkCore;
using TrustTrade.DAL.Abstract;
using TrustTrade.Models;

namespace TrustTrade.DAL.Concrete;

public class UserRepository : Repository<User>, IUserRepository
{
    private DbSet<User> _users;

    public UserRepository(TrustTradeDbContext context) : base(context)
    {
        _users = context.Users;
    }

    public User? FindByIdentityId(string identityId)
    {
        return _users
            .Include(u => u.Comments)
            .Include(u => u.FollowerFollowerUsers)
            .FirstOrDefault(u => u.IdentityId == identityId);
    }
}
