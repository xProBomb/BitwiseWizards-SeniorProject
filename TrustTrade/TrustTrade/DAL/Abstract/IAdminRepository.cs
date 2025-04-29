using TrustTrade.Models;


public interface IAdminRepository
{
    Task<User?> GetByUsernameAsync(string username);
    Task<List<User>> GetAllAsync();
    Task<List<User>> SearchByUsernameAsync(string searchTerm);
    Task<User?> FindByIdAsync(int id);
    Task UpdateAsync(User user);
    Task<User?> GetByIdentityIdAsync(string identityId);

}
