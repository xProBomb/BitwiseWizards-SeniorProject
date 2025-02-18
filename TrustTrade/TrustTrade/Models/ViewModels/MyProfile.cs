// File: TrustTrade/Models/ViewModels/MyProfile.cs

namespace TrustTrade.ViewModels;

public class MyProfileViewModel
{
    public string IdentityId { get; set; }
    public string ProfileName { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? Bio { get; set; }
    public bool IsVerified { get; set; }
    public bool PlaidEnabled { get; set; }
    public DateTime? LastPlaidSync { get; set; }
    public int FollowersCount { get; set; }
    public int FollowingCount { get; set; }
    
    // New properties for holdings
    public List<HoldingViewModel> Holdings { get; set; } = new();
    public DateTime? LastHoldingsUpdate { get; set; }
    public decimal TotalPortfolioValue => Holdings.Sum(h => h.CurrentValue);
}

public class HoldingViewModel
{
    public string Symbol { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal CostBasis { get; set; }
    public decimal CurrentValue => Quantity * CurrentPrice;
    public decimal ReturnAmount => CurrentValue - (Quantity * CostBasis);
    public decimal ReturnPercentage => CostBasis != 0 ? (((ReturnAmount - (CostBasis * Quantity)) / (CostBasis * Quantity)) + 1 ): 0;
    public string Institution { get; set; } = string.Empty;
}