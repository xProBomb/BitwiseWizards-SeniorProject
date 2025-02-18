using System;
using System.Collections.Generic;

namespace TrustTrade.Models;

public partial class Stock
{
    public int Id { get; set; }

    public string TickerSymbol { get; set; } = null!;

    public decimal StockPrice { get; set; }

    public DateTime? LastUpdated { get; set; }

    public virtual ICollection<Trade> Trades { get; set; } = new List<Trade>();
}
