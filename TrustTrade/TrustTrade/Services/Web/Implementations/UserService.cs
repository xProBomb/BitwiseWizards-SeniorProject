using TrustTrade.Models;
using TrustTrade.DAL.Abstract;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using TrustTrade.Services.Web.Interfaces;

namespace TrustTrade.Services.Web.Implementations;

/// <summary>
/// Service for user-related operations.
/// </summary>
public class UserService : IUserService
{
    private readonly ILogger<UserService> _logger;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IUserRepository _userRepository;

    public UserService(ILogger<UserService> logger, UserManager<IdentityUser> userManager, IUserRepository userRepository)
    {
        _logger = logger;
        _userManager = userManager;
        _userRepository = userRepository;
    }

    public async Task<User?> GetCurrentUserAsync(ClaimsPrincipal user, bool includeRelated = false)
    {
        // Get the identity user ID from the UserManager
        string? identityUserId = _userManager.GetUserId(user);
        if (identityUserId == null)
        {
            _logger.LogWarning("Failed to retrieve identity user ID from ClaimsPrincipal.");
            return null;
        }

        // Retrieve the user from the main database
        User? currentUser = await _userRepository.FindByIdentityIdAsync(identityUserId, includeRelated);
        if (currentUser == null)
        {
            _logger.LogWarning($"User with IdentityId '{identityUserId}' not found in the database.");
            return null;
        }
        
        _logger.LogInformation($"Successfully retrieved user '{currentUser.Username}' with IdentityId '{identityUserId}'.");
        return currentUser;
    }
}
