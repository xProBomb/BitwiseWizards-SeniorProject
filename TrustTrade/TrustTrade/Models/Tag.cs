﻿using System;
using System.Collections.Generic;

namespace TrustTrade.Models;

public partial class Tag
{
    public int Id { get; set; }

    public string TagName { get; set; } = null!;

    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
}
