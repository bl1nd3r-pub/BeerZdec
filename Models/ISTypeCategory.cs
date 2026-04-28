using System;
using System.Collections.Generic;

namespace BeerZdec.Models;

public partial class ISTypeCategory
{
    public int ISTC_ID { get; set; }

    public string ISTC_Name { get; set; } = null!;

    public string? ISTC_Descr { get; set; }

    public virtual ICollection<ISType> ISTypes { get; set; } = new List<ISType>();
}
