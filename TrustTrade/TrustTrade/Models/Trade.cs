using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TrustTrade.Models;

[Table("Trade")]
public partial class Trade
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("UserID")]
    public int UserId { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string TickerSymbol { get; set; } = null!;

    [StringLength(20)]
    [Unicode(false)]
    public string TradeType { get; set; } = null!;

    [Column(TypeName = "decimal(10, 2)")]
    public decimal Quantity { get; set; }

    [Column(TypeName = "decimal(13, 2)")]
    public decimal EntryPrice { get; set; }

    [Column(TypeName = "decimal(13, 2)")]
    public decimal CurrentPrice { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? LastUpdated { get; set; }

    [ForeignKey("TickerSymbol")]
    [InverseProperty("Trades")]
    public virtual Stock TickerSymbolNavigation { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("Trades")]
    public virtual User User { get; set; } = null!;
}
