using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrustTrade.Models
{
    public class NotificationSettings
    {
        [Key]
        public int Id { get; set; }
        
        public int UserId { get; set; }
        
        public bool EnableFollowNotifications { get; set; } = true;
        
        public bool EnableLikeNotifications { get; set; } = true;
        
        public bool EnableCommentNotifications { get; set; } = true;
        
        public bool EnableMentionNotifications { get; set; } = true;
        
        public bool EnableMessageNotifications { get; set; } = true;
        
        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}