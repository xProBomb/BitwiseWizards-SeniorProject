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
                    CreatedAt = DateTime.Now
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
    
    [HttpPost]
    public async Task<IActionResult> DeleteConnection()
    {
        try
        {
            var identityUser = await _userManager.GetUserAsync(User);
            if (identityUser == null) return Unauthorized();

            // Get our local user record
            var trustTradeUser = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Email == identityUser.Email);
        
            if (trustTradeUser == null)
            {
                _logger.LogError("Failed to find TrustTrade user record for {Email}", identityUser.Email);
                return StatusCode(500, new { error = "User account not properly synchronized" });
            }

            // Get all Plaid connections for this user
            var plaidConnections = await _dbContext.PlaidConnections
                .Where(pc => pc.UserId == trustTradeUser.Id)
                .ToListAsync();

            // Remove investment positions first to avoid foreign key constraints
            var investments = await _dbContext.InvestmentPositions
                .Where(ip => plaidConnections.Select(pc => pc.Id).Contains(ip.PlaidConnectionId))
                .ToListAsync();
        
            _dbContext.InvestmentPositions.RemoveRange(investments);

            // Remove Plaid connections
            _dbContext.PlaidConnections.RemoveRange(plaidConnections);

            // Update user's Plaid status
            trustTradeUser.PlaidEnabled = false;
            trustTradeUser.PlaidStatus = "Not Connected";
            trustTradeUser.LastPlaidSync = null;

            await _dbContext.SaveChangesAsync();

            // For AJAX requests
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true });
            }

            // For regular form submissions
            return RedirectToAction("Connect");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting Plaid connection");
            return StatusCode(500, new { error = "Failed to delete Plaid connection", details = ex.Message });
        }
    }
}