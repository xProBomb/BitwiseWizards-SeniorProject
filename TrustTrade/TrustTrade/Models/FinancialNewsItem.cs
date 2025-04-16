using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrustTrade.Models;

public partial class FinancialNewsItem
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = null!;

    public string? Summary { get; set; }

    [Required]
    [MaxLength(500)]
    public string Url { get; set; } = null!;

    [Required]
    public DateTime TimePublished { get; set; }

    [MaxLength(255)]
    public string? Authors { get; set; }

    [Required]
    [MaxLength(100)]
    public string Source { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string Category { get; set; } = null!; // 'Stock' or 'Crypto'

    public float? OverallSentimentScore { get; set; }

    [MaxLength(50)]
    public string? OverallSentimentLabel { get; set; }

    [Required]
    public DateTime FetchedDate { get; set; } = DateTime.UtcNow;

    [Required]
    public bool IsActive { get; set; } = true;

    // Navigation properties - must be virtual for proxies
    public virtual ICollection<FinancialNewsTopic> Topics { get; set; } = new List<FinancialNewsTopic>();
    public virtual ICollection<FinancialNewsTickerSentiment> TickerSentiments { get; set; } = new List<FinancialNewsTickerSentiment>();
}