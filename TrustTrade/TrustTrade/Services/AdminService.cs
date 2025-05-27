using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TrustTrade.DAL.Abstract;
using TrustTrade.Models;

public class AdminService : IAdminService
{
    private readonly IAdminRepository _adminRepository;
    private readonly ISiteSettingsRepository _siteSettingsRepository;
    private readonly ILogger<AdminService> _logger;

    public AdminService(
        IAdminRepository adminRepository,
        ISiteSettingsRepository siteSettingsRepository,
        ILogger<AdminService> logger)
    {
        _adminRepository = adminRepository;
        _siteSettingsRepository = siteSettingsRepository;
        _logger = logger;
    }

    public async Task<User?> GetCurrentUserAsync(ClaimsPrincipal principal)
    {
        // Use the NameIdentifier claim to match IdentityId
        var identityId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(identityId))
            return null;

        return await _adminRepository.GetByIdentityIdAsync(identityId);
    }

    public async Task<List<User>> GetAllTrustTradeUsersAsync()
    {
        return await _adminRepository.GetAllAsync();
    }

    public async Task<List<User>> SearchTrustTradeUsersAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await _adminRepository.GetAllAsync();

        return await _adminRepository.SearchByUsernameAsync(searchTerm);
    }

    public async Task SuspendUserAsync(int userId)
    {
        var user = await _adminRepository.FindByIdAsync(userId);
        _logger.LogInformation($"Suspending user with ID: {userId}\n\n\n\n\n");

        if (user == null) return;
        user.PastUsername = user.Username;
        user.Username = "SuspendedUser";
        user.Is_Suspended = true;
        await _adminRepository.UpdateAsync(user);
    }

    public async Task UnsuspendUserAsync(int userId)
    {
        var user = await _adminRepository.FindByIdAsync(userId);
        if (user == null) return;
        user.Username = user.PastUsername ?? "DefaultUser"; // Fallback to a default name if PastUsername is null
        user.Is_Suspended = false;
        await _adminRepository.UpdateAsync(user);
    }

    public async Task<User?> FindUserByIdAsync(int userId)
    {
        return await _adminRepository.FindByIdAsync(userId);
    }

    public async Task<SiteSettings> GetSiteSettingsAsync()
    {
        return await _siteSettingsRepository.GetSiteSettingsAsync();
    }

    public async Task EnablePresentationModeAsync()
    {
        var settings = await _siteSettingsRepository.GetSiteSettingsAsync();
        settings.IsPresentationModeEnabled = true;
        await _siteSettingsRepository.AddOrUpdateAsync(settings);
    }

    public async Task DisablePresentationModeAsync()
    {
        var settings = await _siteSettingsRepository.GetSiteSettingsAsync();
        settings.IsPresentationModeEnabled = false;
        await _siteSettingsRepository.AddOrUpdateAsync(settings);
    }
}
