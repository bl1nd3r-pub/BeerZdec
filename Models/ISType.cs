using System;
using System.Collections.Generic;

namespace BeerZdec.Models;

public partial class ISType
{
    public int ISType_ID { get; set; }

    public string ISType_Descr { get; set; } = null!;

    public int ISType_Category { get; set; }

    public string ISType_Name { get; set; } = null!;

    public double? ISType_ApplicationRate { get; set; }

    public double? ISType_EfficiencyCoefficient { get; set; }

    public double? ISType_InstallationCostPerHa { get; set; }

    public DateOnly? ISType_LastUpdated { get; set; }

    public virtual ISTypeCategory ISType_CategoryNavigation { get; set; } = null!;

    public virtual ICollection<SowingPlot> SowingPlots { get; set; } = new List<SowingPlot>();
}
