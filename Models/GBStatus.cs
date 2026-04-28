using System;
using System.Collections.Generic;

namespace BeerZdec.Models;

public partial class GBStatus
{
    public int GBStatus_ID { get; set; }

    public string? GBStatus_Name { get; set; }

    public virtual ICollection<GrainBatch> GrainBatches { get; set; } = new List<GrainBatch>();
}
