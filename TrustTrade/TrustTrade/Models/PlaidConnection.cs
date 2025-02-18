using System;
using System.Collections.Generic;

namespace TrustTrade.Models;

public partial class PlaidConnection
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string ItemId { get; set; } = null!;

    public string AccessToken { get; set; } = null!;

    public string InstitutionId { get; set; } = null!;

    public string InstitutionName { get; set; } = null!;

    public DateTime? LastSyncTimestamp { get; set; }

    public virtual ICollection<InvestmentPosition> InvestmentPositions { get; set; } = new List<InvestmentPosition>();

    public virtual User User { get; set; } = null!;
}
