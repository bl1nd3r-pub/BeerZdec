using System;
using System.Collections.Generic;

namespace BeerZdec.Models;

public partial class HarvestEvent
{
    public int HarvestEvent_ID { get; set; }

    public int HarvestEvent_SowPlot { get; set; }

    public DateOnly? HarvestEvent_Date { get; set; }

    public double? HarvestEvent_GrossWeight { get; set; }

    public virtual ICollection<GrainBatch> GrainBatches { get; set; } = new List<GrainBatch>();

    public virtual SowingPlot HarvestEvent_SowPlotNavigation { get; set; } = null!;
}
