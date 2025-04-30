using System.ComponentModel.DataAnnotations;

namespace TrustTrade.Models.ViewModels
{
    public class NotificationSettingsViewModel
    {
        [Display(Name = "Follow Notifications")]
        public bool EnableFollowNotifications { get; set; } = true;
        
        [Display(Name = "Like Notifications")]
        public bool EnableLikeNotifications { get; set; } = true;
        
        [Display(Name = "Comment Notifications")]
        public bool EnableCommentNotifications { get; set; } = true;
        
        [Display(Name = "Mention Notifications")]
        public bool EnableMentionNotifications { get; set; } = true;
        
        [Display(Name = "Message Notifications")]
        public bool EnableMessageNotifications { get; set; } = true;
    }
}