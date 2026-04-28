using System;
using System.Collections.Generic;

namespace BeerZdec.Models;

public partial class GBQualGrade
{
    public int GBQualGrade_ID { get; set; }

    public string? GBQualGrade_Name { get; set; }

    public virtual ICollection<GrainBatch> GrainBatches { get; set; } = new List<GrainBatch>();
}
