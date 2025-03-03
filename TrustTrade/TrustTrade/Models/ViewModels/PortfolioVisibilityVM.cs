using System.Collections.Generic;

namespace TrustTrade.Models.ViewModels
{
    public class PortfolioVisibilityViewModel
    {
        public bool HideDetailedInformation { get; set; }
        public bool HideAllPositions { get; set; }
        public List<HoldingVisibilityViewModel> Holdings { get; set; } = new();
    }

    public class HoldingVisibilityViewModel
    {
        public int Id { get; set; }
        public string Symbol { get; set; } = string.Empty;
        public bool IsHidden { get; set; }
    }
}