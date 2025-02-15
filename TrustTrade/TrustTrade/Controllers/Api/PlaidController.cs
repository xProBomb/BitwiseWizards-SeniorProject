using Going.Plaid;
using Going.Plaid.Link;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
    [HttpPost]
    public async Task<IActionResult> ExchangePublicToken([FromBody] string publicToken)
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var exchangeResponse = await _plaidClient.ItemPublicTokenExchangeAsync(
                new Going.Plaid.Item.ItemPublicTokenExchangeRequest
                {
                    PublicToken = publicToken
                });

            // Save the Plaid connection in our database
            var plaidConnection = new PlaidConnection
            {
                UserId = int.Parse(user.Id), // Ensure your User table uses int IDs
                AccessToken = exchangeResponse.AccessToken,
                ItemId = exchangeResponse.ItemId,
                // Set other fields as needed
                LastSyncTimestamp = DateTime.UtcNow
            };

            _dbContext.PlaidConnections.Add(plaidConnection);
            await _dbContext.SaveChangesAsync();

            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exchanging public token");
            return StatusCode(500, new { error = "Failed to exchange public token" });
        }
    }
}