using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrustTrade.Models
{
    public class Conversation
    {
        [Key]
        public int Id { get; set; }
        
        // The two participants in the conversation
        public int User1Id { get; set; }
        public int User2Id { get; set; }
        
        // Navigation properties for the participants
        [ForeignKey("User1Id")]
        public virtual User User1 { get; set; }
        
        [ForeignKey("User2Id")]
        public virtual User User2 { get; set; }
        
        // Last message in the conversation for preview
        public string LastMessageContent { get; set; }
        
        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Collection of messages in this conversation
        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}