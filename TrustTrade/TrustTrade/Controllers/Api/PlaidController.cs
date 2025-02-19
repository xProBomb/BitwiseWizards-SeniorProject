using Going.Plaid;
using Going.Plaid.Link;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrustTrade.Models;

namespace TrustTrade.Controllers.Api;

/// <summary>
/// Response model for Link token creation
/// </summary>
public class LinkTokenResponse
{
    /// <summary>
    /// The Plaid Link token used to initialize Link
    /// </summary>
    public string LinkToken { get; set; } = string.Empty;
}

/// <summary>
/// Controller for handling Plaid Link integration and brokerage connections
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PlaidController : ControllerBase
{
    private readonly PlaidClient _plaidClient;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly TrustTradeDbContext _dbContext;
    private readonly ILogger<PlaidController> _logger;

    /// <summary>
    /// Constructor for PlaidController
    /// </summary>
    public PlaidController(
        PlaidClient plaidClient,
        UserManager<IdentityUser> userManager,
        TrustTradeDbContext dbContext,
        ILogger<PlaidController> logger)
    {
        _plaidClient = plaidClient;
        _userManager = userManager;
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Creates a Link token for initializing Plaid Link
    /// </summary>
    /// <returns>JSON response containing the link token</returns>
    [HttpPost("create-link-token")]
    public async Task<ActionResult<LinkTokenResponse>> CreateLinkToken()
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var request = new LinkTokenCreateRequest
            {
                User = new Going.Plaid.Entity.LinkTokenCreateRequestUser
                {
                    ClientUserId = user.Id
                },
                ClientName = "TrustTrade",
                Products = new[] { Going.Plaid.Entity.Products.Investments },
                CountryCodes = new[] { Going.Plaid.Entity.CountryCode.Us },
                Language = Going.Plaid.Entity.Language.English,
                Investments = new Going.Plaid.Entity.LinkTokenInvestments
                {
                    AllowManualEntry = true
                }
            };

            var response = await _plaidClient.LinkTokenCreateAsync(request);
            return Ok(new LinkTokenResponse { LinkToken = response.LinkToken });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Link token");
            return StatusCode(500, new { error = "Failed to create Link token" });
        }
    }

    /// <summary>
    /// Handles the exchange of public token after successful Plaid Link flow
    /// </summary>
    /// <param name="publicToken">The public token from Plaid Link</param>
    /// <returns>Success or error response</returns>
    /// <summary>
/// Request model for public token exchange
/// </summary>
public class ExchangePublicTokenRequest
{
    /// <summary>
    /// The public token received from Plaid Link
    /// </summary>
    public string PublicToken { get; set; } = string.Empty;
}

[HttpPost("exchange-public-token")]
public async Task<IActionResult> ExchangePublicToken([FromBody] ExchangePublicTokenRequest request)
{
    try
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        // Get our local user record
        var trustTradeUser = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == user.Email);
        
        if (trustTradeUser == null)
        {
            _logger.LogError("Failed to find TrustTrade user record for {Email}", user.Email);
            return StatusCode(500, new { error = "User account not properly synchronized" });
        }

        // Exchange the public token
        var exchangeResponse = await _plaidClient.ItemPublicTokenExchangeAsync(
            new Going.Plaid.Item.ItemPublicTokenExchangeRequest
            {
                PublicToken = request.PublicToken
            });

        // Get institution details
        var itemResponse = await _plaidClient.ItemGetAsync(
            new Going.Plaid.Item.ItemGetRequest
            {
                AccessToken = exchangeResponse.AccessToken
            });

        var institutionResponse = await _plaidClient.InstitutionsGetByIdAsync(
            new Going.Plaid.Institutions.InstitutionsGetByIdRequest
            {
                InstitutionId = itemResponse.Item.InstitutionId,
                CountryCodes = new[] { Going.Plaid.Entity.CountryCode.Us }
            });

        // Save the Plaid connection
        var plaidConnection = new PlaidConnection
        {
            UserId = trustTradeUser.Id,
            AccessToken = exchangeResponse.AccessToken,
            ItemId = exchangeResponse.ItemId,
            InstitutionId = itemResponse.Item.InstitutionId,
            InstitutionName = institutionResponse.Institution.Name,
            LastSyncTimestamp = DateTime.Now
        };

        _dbContext.PlaidConnections.Add(plaidConnection);

        // Update user's Plaid status
        trustTradeUser.PlaidEnabled = true;
        trustTradeUser.PlaidStatus = "Connected";
        trustTradeUser.LastPlaidSync = DateTime.Now;

        await _dbContext.SaveChangesAsync();

        return Ok(new { success = true });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error exchanging public token");
        return StatusCode(500, new { error = "Failed to exchange public token", details = ex.Message });
    }
}
}