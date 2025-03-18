using TrustTrade.Models;

namespace TrustTrade.DAL.Abstract;

public interface IUserRepository : IRepository<User>
{
    Task<User?> FindByIdentityIdAsync(string identityId);
}