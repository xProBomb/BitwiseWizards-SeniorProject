using System;
using System.Collections.Generic;

namespace TrustTrade.Models;

public partial class Trade
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string TickerSymbol { get; set; } = null!;

    public string TradeType { get; set; } = null!;

    public decimal Quantity { get; set; }

    public decimal EntryPrice { get; set; }

    public decimal CurrentPrice { get; set; }

    public DateTime? LastUpdated { get; set; }

    public virtual Stock TickerSymbolNavigation { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
