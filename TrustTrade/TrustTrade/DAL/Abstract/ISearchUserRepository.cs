using System.Collections.Generic;
using System.Threading.Tasks;
using TrustTrade.Models;

namespace TrustTrade.Data
{
    public interface ISearchUserRepository
    {
        Task<IEnumerable<User>> SearchUsersAsync(string searchTerm);
        
    }
}
