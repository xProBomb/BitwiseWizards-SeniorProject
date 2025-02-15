using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TrustTrade.Models;
using TrustTrade.Models.ViewModels;

namespace TrustTrade.Controllers;

/// <summary>
/// Controller for managing brokerage connections
/// </summary>
[Authorize]
public class BrokerageController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly TrustTradeDbContext _dbContext;
    private readonly ILogger<BrokerageController> _logger;

    /// <summary>
    /// Constructor for BrokerageController
    /// </summary>
    public BrokerageController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        TrustTradeDbContext dbContext,
        ILogger<BrokerageController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Displays the brokerage connection page
    /// </summary>
    public async Task<IActionResult> Connect()
    {
        // Detailed authentication state logging
        _logger.LogInformation("Connect action accessed. Authentication state: {IsAuthenticated}", 
            User.Identity?.IsAuthenticated);
        _logger.LogInformation("Current user identity name: {Name}", 
            User.Identity?.Name ?? "null");
        _logger.LogInformation("Current user claims: {Claims}", 
            string.Join(", ", User.Claims.Select(c => $"{c.Type}: {c.Value}")));

        if (!User.Identity?.IsAuthenticated ?? true)
        {
            _logger.LogWarning("User not authenticated, redirecting to login");
            return RedirectToPage("/Account/Login", new { area = "Identity" });
        }

        try
        {
            var identityUser = await _userManager.GetUserAsync(User);
            if (identityUser == null)
            {
                _logger.LogError("Failed to get Identity user details despite authenticated state");
                // Instead of signing out, let's try to recover the user
                var email = User.FindFirstValue(ClaimTypes.Email);
                if (!string.IsNullOrEmpty(email))
                {
                    identityUser = await _userManager.FindByEmailAsync(email);
                    if (identityUser == null)
                    {
                        _logger.LogError("Unable to recover user by email: {Email}", email);
                        return RedirectToPage("/Account/Login", new { area = "Identity" });
                    }
                }
                else
                {
                    return RedirectToPage("/Account/Login", new { area = "Identity" });
                }
            }

            // Synchronize with TrustTrade DB
            var trustTradeUser = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Email == identityUser.Email);

            if (trustTradeUser == null)
            {
                _logger.LogInformation("Creating new TrustTrade user for {Email}", identityUser.Email);
                trustTradeUser = new User
                {
                    Email = identityUser.Email!,
                    Username = identityUser.UserName ?? identityUser.Email!,
                    ProfileName = identityUser.UserName ?? identityUser.Email!,
                    PasswordHash = "[MANAGED_BY_IDENTITY]",
                    CreatedAt = DateTime.UtcNow
                };

                _dbContext.Users.Add(trustTradeUser);
                await _dbContext.SaveChangesAsync();
            }

            var existingConnection = await _dbContext.PlaidConnections
                .FirstOrDefaultAsync(p => p.UserId == trustTradeUser.Id);

            var viewModel = new BrokerageConnectionViewModel
            {
                UserEmail = identityUser.Email!,
                HasExistingConnection = existingConnection != null,
                ExistingConnection = existingConnection
            };

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Connect action");
            throw;
        }
    }
}