namespace TrustTrade.ViewModels;

public class ProfileViewModel
{
    public string? UserTag { get; set; }
    public string IdentityId { get; set; }
    public string ProfileName { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? Bio { get; set; }
    public bool IsVerified { get; set; }
    public bool PlaidEnabled { get; set; }
    public DateTime? LastPlaidSync { get; set; }
    public int FollowersCount { get; set; }
    public int FollowingCount { get; set; }
    public List<string> Followers { get; set; } = new();
    public List<string> Following { get; set; } = new();
    public bool IsFollowing { get; set; }

    public bool HideDetailedInformation { get; set; }
    public bool HideAllPositions { get; set; }
    public decimal HiddenAssetsValue => Holdings.Where(h => h.IsHidden).Sum(h => h.CurrentValue);
    public int HiddenAssetsCount => Holdings.Count(h => h.IsHidden);
    public decimal VisibleAssetsValue => Holdings.Where(h => !h.IsHidden).Sum(h => h.CurrentValue);
    
    // New properties for performance
    public decimal PerformanceScore { get; set; }
    public bool HasRatedScore { get; set; }
    public Dictionary<string, decimal> ScoreBreakdown { get; set; } = new();
    
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
    public decimal ReturnPercentage => CostBasis != 0 ? (100 *((CurrentValue - (Quantity * CostBasis)) / (Quantity * CostBasis))) : 0;
    public string Institution { get; set; } = string.Empty;
    public string TypeOfSecurity { get; set; } = string.Empty;

    public bool IsHidden { get; set; }
}