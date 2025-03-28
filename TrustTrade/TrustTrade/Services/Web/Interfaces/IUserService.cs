using TrustTrade.Models;
using System.Security.Claims;

namespace TrustTrade.Services.Web.Interfaces;

public interface IUserService
{
    Task<User?> GetCurrentUserAsync(ClaimsPrincipal user);
}