using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrustTrade.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }
        
        // Foreign keys
        public int ConversationId { get; set; }
        public int SenderId { get; set; }
        public int RecipientId { get; set; }
        
        // Navigation properties
        [ForeignKey("ConversationId")]
        public virtual Conversation Conversation { get; set; }
        
        [ForeignKey("SenderId")]
        public virtual User Sender { get; set; }
        
        [ForeignKey("RecipientId")]
        public virtual User Recipient { get; set; }
        
        // Message content
        [Required]
        public string Content { get; set; }
        
        // Message status
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
        public DateTime? ReadAt { get; set; }
        
        // Status flags
        public bool IsDeleted { get; set; } = false;
        public bool IsDeletedForSender { get; set; } = false;
        public bool IsDeletedForRecipient { get; set; } = false;
    }
}