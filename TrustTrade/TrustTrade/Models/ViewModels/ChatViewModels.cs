namespace TrustTrade.Models.ViewModels
{
    public class ChatConversationVM
    {
        public int Id { get; set; }
        public int OtherUserId { get; set; }
        public string OtherUsername { get; set; }
        public byte[] OtherUserProfilePicture { get; set; }
        public string LastMessage { get; set; }
        public DateTime LastMessageTime { get; set; }
        public bool HasUnreadMessages { get; set; }
        public int UnreadCount { get; set; }
    }
    
    public class ChatMessageVM
    {
        public int Id { get; set; }
        public int ConversationId { get; set; }
        public int SenderId { get; set; }
        public string SenderUsername { get; set; }
        public byte[] SenderProfilePicture { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public bool IsOwnMessage { get; set; }
    }
    
    public class ChatIndexVM
    {
        public List<ChatConversationVM> Conversations { get; set; } = new List<ChatConversationVM>();
        public int TotalConversations { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalConversations / (double)PageSize);
    }
    
    public class ChatDetailsVM
    {
        public Conversation Conversation { get; set; }
        public List<ChatMessageVM> Messages { get; set; } = new List<ChatMessageVM>();
        public int CurrentUserId { get; set; }
        public string OtherUsername { get; set; }
        public byte[] OtherUserProfilePicture { get; set; }
        public int OtherUserId { get; set; }
        public int TotalMessages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalMessages / (double)PageSize);
    }
    
    public class SendMessageVM
    {
        public int ConversationId { get; set; }
        public string Content { get; set; }
    }
}