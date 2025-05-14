using System;
using System.Collections.Generic;

namespace TrustTrade.Models;

public partial class Photo
{
    public int Id { get; set; }

    public byte[]? Image { get; set; }

    public int PostId { get; set; }
    
    public DateTime CreatedAt { get; set; }

    public virtual Post Post { get; set; } = null!;
}
