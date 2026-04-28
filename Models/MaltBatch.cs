using System;
using System.Collections.Generic;

namespace BeerZdec.Models;

public partial class MaltBatch
{
    public int MaltBatch_ID { get; set; }

    public int MaltBatch_MaltProcess { get; set; }

    public double? MaltBatch_Quantity { get; set; }

    public virtual ICollection<Crude> Crudes { get; set; } = new List<Crude>();

    public virtual MaltProcess MaltBatch_MaltProcessNavigation { get; set; } = null!;
}
