using System;
using System.Collections.Generic;

namespace BeerZdec.Models;

public partial class SowingProcess
{
    public int SowingProcess_ID { get; set; }

    public DateOnly SowProc_Datetime { get; set; }

    public int SowProc_Variety { get; set; }

    public int SowProc_SowPlot { get; set; }

    // Навигация
    public virtual Variety SowProc_VarietyNavigation { get; set; } = null!;
    public virtual SowingPlot SowProc_SowPlotNavigation { get; set; } = null!;

    // Обратная навигация (если нужно будет связать урожай с конкретным посевом)
    // public virtual ICollection<HarvestEvent> HarvestEvents { get; set; } = new List<HarvestEvent>();
}