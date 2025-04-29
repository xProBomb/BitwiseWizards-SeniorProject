using Microsoft.EntityFrameworkCore;
using TrustTrade.DAL.Abstract;
using TrustTrade.Models;
using System.Security.Claims;

public class AdminService : IAdminService
{
    private readonly IAdminRepository _adminRepository;

    public AdminService(IAdminRepository adminRepository)
    {
        _adminRepository = adminRepository;
    }

    public async Task<User?> GetCurrentUserAsync(ClaimsPrincipal principal)
    {
        var username = principal.Identity?.Name;
        if (string.IsNullOrEmpty(username))
            return null;

        return await _adminRepository.GetByUsernameAsync(username);
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
        if (user == null) return;

        user.IsSuspended = true;
        await _adminRepository.UpdateAsync(user);
    }

    public async Task UnsuspendUserAsync(int userId)
    {
        var user = await _adminRepository.FindByIdAsync(userId);
        if (user == null) return;

        user.IsSuspended = false;
        await _adminRepository.UpdateAsync(user);
    }
}
