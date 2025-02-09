using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TrustTrade.Models;

[Index("Username", Name = "UQ__Users__536C85E432493D5E", IsUnique = true)]
[Index("Email", Name = "UQ__Users__A9D105343E6C921E", IsUnique = true)]
public partial class User
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("Profile_Name")]
    [StringLength(50)]
    public string ProfileName { get; set; } = null!;

    [StringLength(50)]
    public string Username { get; set; } = null!;

    [StringLength(100)]
    public string Email { get; set; } = null!;

    [StringLength(255)]
    public string PasswordHash { get; set; } = null!;

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [StringLength(255)]
    public string? ProfilePicture { get; set; }

    [StringLength(500)]
    public string? Bio { get; set; }

    [Column("Is_Admin")]
    public bool? IsAdmin { get; set; }

    [Column("Is_Verified")]
    public bool? IsVerified { get; set; }

    [Column("EncryptedAPIKey")]
    public byte[]? EncryptedApikey { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    [InverseProperty("FollowerUser")]
    public virtual ICollection<Follower> FollowerFollowerUsers { get; set; } = new List<Follower>();

    [InverseProperty("FollowingUser")]
    public virtual ICollection<Follower> FollowerFollowingUsers { get; set; } = new List<Follower>();

    [InverseProperty("User")]
    public virtual ICollection<Like> Likes { get; set; } = new List<Like>();

    [InverseProperty("User")]
    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

    [InverseProperty("User")]
    public virtual ICollection<Trade> Trades { get; set; } = new List<Trade>();
}
