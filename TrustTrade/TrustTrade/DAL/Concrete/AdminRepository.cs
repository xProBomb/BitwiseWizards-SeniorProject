using Microsoft.EntityFrameworkCore;
using TrustTrade.Models;

public class AdminRepository : IAdminRepository
{
    private readonly TrustTradeDbContext _context;
    public AdminRepository(TrustTradeDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await _context.Users.ToListAsync();
    }

    public async Task<List<User>> SearchByUsernameAsync(string searchTerm)
    {
        return await _context.Users
            .Where(u => u.Username.Contains(searchTerm))
            .ToListAsync();
    }

    public async Task<User?> FindByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task<User?> GetByIdentityIdAsync(string identityId)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.IdentityId == identityId);
    }

}
