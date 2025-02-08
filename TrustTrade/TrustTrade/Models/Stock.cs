using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TrustTrade.Models;

[Table("Stock")]
[Index("TickerSymbol", Name = "UQ__Stock__F144591BE1EFDFAD", IsUnique = true)]
public partial class Stock
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [StringLength(10)]
    [Unicode(false)]
    public string TickerSymbol { get; set; } = null!;

    [Column(TypeName = "decimal(13, 2)")]
    public decimal StockPrice { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? LastUpdated { get; set; }

    [InverseProperty("TickerSymbolNavigation")]
    public virtual ICollection<Trade> Trades { get; set; } = new List<Trade>();
}
