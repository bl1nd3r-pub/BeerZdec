using System;
using System.Collections.Generic;

namespace BeerZdec.Models;

public partial class Grain
{
    public int Grain_ID { get; set; }

    public string? Grain_NameRu { get; set; }

    public string? Grain_NameLatin { get; set; }

    public virtual ICollection<Variety> Varieties { get; set; } = new List<Variety>();
}
