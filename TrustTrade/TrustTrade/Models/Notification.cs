using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrustTrade.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }
        
        public int UserId { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Type { get; set; } // Follow, Like, Comment, Mention
        
        [Required]
        public string Message { get; set; }
        
        // The entity ID that this notification is about (PostId, CommentId, etc.)
        public int? EntityId { get; set; }
        
        // Type of the entity (Post, Comment, User, etc.)
        [MaxLength(50)]
        public string EntityType { get; set; }
        
        // The user who triggered this notification (if applicable)
        public int? ActorId { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsRead { get; set; } = false;
        
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        
        [ForeignKey("ActorId")]
        public virtual User Actor { get; set; }
    }
}