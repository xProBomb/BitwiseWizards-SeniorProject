using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TrustTrade.Models;

public partial class Like
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("UserID")]
    public int UserId { get; set; }

    [Column("PostID")]
    public int PostId { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [ForeignKey("PostId")]
    [InverseProperty("Likes")]
    public virtual Post Post { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("Likes")]
    public virtual User User { get; set; } = null!;
}
