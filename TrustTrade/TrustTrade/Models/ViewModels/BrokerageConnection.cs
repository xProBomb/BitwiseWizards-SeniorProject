namespace TrustTrade.Models.ViewModels;

/// <summary>
/// View model for the brokerage connection page
/// </summary>
public class BrokerageConnectionViewModel
{
    /// <summary>
    /// The user's email address from Identity
    /// </summary>
    public string UserEmail { get; set; } = string.Empty;

    /// <summary>
    /// Whether the user has already connected a brokerage
    /// </summary>
    public bool HasExistingConnection { get; set; }

    /// <summary>
    /// Details about the connected brokerage, if any
    /// </summary>
    public PlaidConnection? ExistingConnection { get; set; }
}