using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrustTrade.Models;

namespace TrustTrade.Data
{
    public class SearchUserRepository : ISearchUserRepository
    {
        private readonly TrustTradeDbContext _context; 

        public SearchUserRepository(TrustTradeDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> SearchUsersAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return new List<User>();
            }

            // Basic case-insensitive search by username
            return await _context.Users
                .Where(u => u.Username.Contains(searchTerm))
                .OrderBy(u => u.Username)
                .ToListAsync();
        }
    }
}
