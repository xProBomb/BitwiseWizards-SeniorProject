using System;
using System.Collections.Generic;

namespace TrustTrade.Models;

public partial class Follower
{
    public int Id { get; set; }

    public int FollowerUserId { get; set; }

    public int FollowingUserId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User FollowerUser { get; set; } = null!;

    public virtual User FollowingUser { get; set; } = null!;
}
