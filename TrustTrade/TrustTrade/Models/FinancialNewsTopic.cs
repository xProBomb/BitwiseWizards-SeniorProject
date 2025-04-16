using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrustTrade.Models;

public partial class FinancialNewsTopic
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int NewsItemId { get; set; }

    [Required]
    [MaxLength(100)]
    public string TopicName { get; set; } = null!;

    [Required]
    public float RelevanceScore { get; set; }

    // Navigation property - must be virtual for proxies
    [ForeignKey("NewsItemId")]
    public virtual FinancialNewsItem? NewsItem { get; set; }
}