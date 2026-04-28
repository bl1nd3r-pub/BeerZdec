using System;
using System.Collections.Generic;

namespace BeerZdec.Models;

public partial class User
{
    public int Id { get; set; }

    public string UsLogin { get; set; } = null!;

    public string UsPassword { get; set; } = null!;

    public string? Role { get; set; }
}
