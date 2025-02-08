using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TrustTrade.Models;

public partial class Follower
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("FollowerUserID")]
    public int FollowerUserId { get; set; }

    [Column("FollowingUserID")]
    public int FollowingUserId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [ForeignKey("FollowerUserId")]
    [InverseProperty("FollowerFollowerUsers")]
    public virtual User FollowerUser { get; set; } = null!;

    [ForeignKey("FollowingUserId")]
    [InverseProperty("FollowerFollowingUsers")]
    public virtual User FollowingUser { get; set; } = null!;
}
