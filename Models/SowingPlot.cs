using System;
using System.Collections.Generic;

namespace BeerZdec.Models;

public partial class SowingPlot
{
    public int SowingPlot_ID { get; set; }

    public int SowPlot_SoilType { get; set; }

    public int SowPlot_IrrigationSystemType { get; set; }

    public double? SowPlot_Square { get; set; }

    public virtual ICollection<HarvestEvent> HarvestEvents { get; set; } = new List<HarvestEvent>();

    public virtual ISType SowPlot_IrrigationSystemTypeNavigation { get; set; } = null!;

    public virtual SoilType SowPlot_SoilTypeNavigation { get; set; } = null!;
}
