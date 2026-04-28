using System;
using System.Collections.Generic;

namespace BeerZdec.Models;

public partial class BeerStyle
{
    public int BeerStyle_ID { get; set; }

    public string BeerStyle_Name { get; set; } = null!;

    public string? BeerStyle_Description { get; set; }

    public double? BeerStyle_TargetOG { get; set; }

    public double? BeerStyle_TargetFG { get; set; }

    public double? BeerStyle_TargetABV { get; set; }

    public int? BeerStyle_TargetIBU { get; set; }

    public int? BeerStyle_TargetColorEBC { get; set; }

    public bool? BeerStyle_IsActive { get; set; }

    public virtual ICollection<BrewingBatch> BrewingBatches { get; set; } = new List<BrewingBatch>();
}
