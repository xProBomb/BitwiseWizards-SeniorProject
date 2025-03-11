using TrustTrade.Models;

namespace TrustTrade.DAL.Abstract;

public interface IUserRepository : IRepository<User>
{
    User? FindByIdentityId(string identityId);
}