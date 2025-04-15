using TrustTrade.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace TrustTrade.Services.Web.Interfaces;

/// <summary>
/// Interface for user-related services.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Get the current user based on the ClaimsPrincipal. To be used with the User property in controllers.
    /// </summary>
    /// <param name="user">The ClaimsPrincipal representing the current user.</param>
    /// <param name="includeRelated">Whether to include related entities.</param>
    /// <returns>The current user, or null if not found.</returns>
    Task<User?> GetCurrentUserAsync(ClaimsPrincipal user, bool includeRelated = false);

    /// <summary>
    /// Get a user by their username.
    /// </summary>
    /// <param name="username">The username of the user to find.</param>
    /// <returns>The user with the specified username, or null if not found.</returns>
    Task<User?> GetUserByUsernameAsync(string username);
}