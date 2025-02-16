using System;

namespace TrustTrade.ViewModels
{
    public class MyProfileViewModel
    {
        public string IdentityId { get; set; }
        public string ProfileName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? Bio { get; set; }
        public bool IsVerified { get; set; }
        public bool PlaidEnabled { get; set; }
        public DateTime? LastPlaidSync { get; set; }
        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }
        
        // Placeholder for Posts - to be implemented later.
        // public IEnumerable<PostViewModel> Posts { get; set; }
    }
}
