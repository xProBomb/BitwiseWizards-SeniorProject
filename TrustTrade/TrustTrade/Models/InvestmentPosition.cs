using System;
using System.Collections.Generic;

namespace TrustTrade.Models;

public partial class InvestmentPosition
{
    public int Id { get; set; }

    public int PlaidConnectionId { get; set; }

    public string SecurityId { get; set; } = null!;

    public string Symbol { get; set; } = null!;

    public decimal Quantity { get; set; }

    public decimal CostBasis { get; set; }

    public decimal CurrentPrice { get; set; }
    
    public string TypeOfSecurity { get; set; }

    public DateTime? LastUpdated { get; set; }

    public virtual PlaidConnection PlaidConnection { get; set; } = null!;
}
