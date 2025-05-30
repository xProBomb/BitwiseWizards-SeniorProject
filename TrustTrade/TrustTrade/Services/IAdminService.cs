using TrustTrade.Models;
using System.Security.Claims;

public interface IAdminService
{
    Task<User?> GetCurrentUserAsync(ClaimsPrincipal user);
    Task<List<User>> GetAllTrustTradeUsersAsync();
    Task<List<User>> SearchTrustTradeUsersAsync(string searchTerm);
    Task SuspendUserAsync(int userId);
    Task UnsuspendUserAsync(int userId);
    Task<User?> FindUserByIdAsync(int userId);
    Task<SiteSettings> GetSiteSettingsAsync();
    Task EnablePresentationModeAsync();
    Task DisablePresentationModeAsync();
    Task AddPresenterAsync(int userId);
    Task RemovePresenterAsync(int userId);
}
