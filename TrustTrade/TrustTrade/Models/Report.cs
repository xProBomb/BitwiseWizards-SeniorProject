using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrustTrade.Models
{
    public class Report
    {
        [Key]
        public int Id { get; set; }

        // Reporter information
        public int ReporterId { get; set; }
        
        // What is being reported
        [Required]
        [MaxLength(50)]
        public string ReportType { get; set; } // "Post" or "Profile"
        
        // ID of the reported entity
        public int? ReportedPostId { get; set; }
        public int? ReportedUserId { get; set; }
        
        // Report details
        [Required]
        [MaxLength(100)]
        public string Category { get; set; }
        
        [MaxLength(1000)]
        public string Description { get; set; }
        
        // Status tracking
        [MaxLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Reviewed, Resolved, Dismissed
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewedAt { get; set; }
        public int? ReviewedByUserId { get; set; }
        
        [MaxLength(500)]
        public string AdminNotes { get; set; }
        
        // Navigation properties
        [ForeignKey("ReporterId")]
        public virtual User Reporter { get; set; }
        
        [ForeignKey("ReportedUserId")]
        public virtual User ReportedUser { get; set; }
        
        [ForeignKey("ReportedPostId")]
        public virtual Post ReportedPost { get; set; }
        
        [ForeignKey("ReviewedByUserId")]
        public virtual User ReviewedByUser { get; set; }
    }
}