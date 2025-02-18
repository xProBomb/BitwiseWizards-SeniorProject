using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace TrustTrade.Models;

public partial class User
{
    public int Id { get; set; }

    [Column("IdentityId")]
    public string? IdentityId { get; set; }

    public string ProfileName { get; set; } = null!;

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public string? ProfilePicture { get; set; }

    public string? Bio { get; set; }

    public bool? IsAdmin { get; set; }

    public bool? IsVerified { get; set; }

    public byte[]? EncryptedApikey { get; set; }

    public bool? PlaidEnabled { get; set; }

    public DateTime? LastPlaidSync { get; set; }

    public string? PlaidStatus { get; set; }

    public string? PlaidSettings { get; set; }

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<Follower> FollowerFollowerUsers { get; set; } = new List<Follower>();

    public virtual ICollection<Follower> FollowerFollowingUsers { get; set; } = new List<Follower>();

    public virtual ICollection<Like> Likes { get; set; } = new List<Like>();

    public virtual ICollection<PlaidConnection> PlaidConnections { get; set; } = new List<PlaidConnection>();

    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

    public virtual ICollection<Trade> Trades { get; set; } = new List<Trade>();
}
