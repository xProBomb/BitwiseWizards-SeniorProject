using TrustTrade.Models;

namespace TrustTrade.DAL.Abstract;

/// <summary>
/// Repository interface for User entities.
/// </summary>
public interface IUserRepository : IRepository<User>
{
    /// <summary>
    /// Find a user by their IdentityId.
    /// </summary>
    /// <param name="identityId">The IdentityId of the user to find.</param>
    /// <param name="includeRelated">Whether to include related entities.</param>
    /// <returns>The user with the specified IdentityId, or null if not found.</returns>
    Task<User?> FindByIdentityIdAsync(string identityId, bool includeRelated = false);
}