using System;
using System.Collections.Generic;

namespace TrustTrade.Models;

public class StockHistory
{
    public int Id { get; set; }
    public string TickerSymbol { get; set; } = null!;
    public DateTime Date { get; set; }
    public decimal HighPrice { get; set; }
    public decimal LowPrice { get; set; }
}
