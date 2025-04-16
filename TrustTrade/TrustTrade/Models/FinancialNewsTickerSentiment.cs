using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrustTrade.Models;

public partial class FinancialNewsTickerSentiment
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int NewsItemId { get; set; }

    [Required]
    [MaxLength(20)]
    public string TickerSymbol { get; set; } = null!;

    public float? TickerSentimentScore { get; set; }

    [MaxLength(50)]
    public string? TickerSentimentLabel { get; set; }

    [Required]
    public float RelevanceScore { get; set; }

    // Navigation property - must be virtual for proxies
    [ForeignKey("NewsItemId")]
    public virtual FinancialNewsItem? NewsItem { get; set; }
}