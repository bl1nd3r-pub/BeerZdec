using System;
using System.Collections.Generic;

namespace BeerZdec.Models;

public partial class BrewIngredient
{
    public int BrewIngr_ID { get; set; }

    public int? BrewIngr_BrewBatch { get; set; }

    public int? BrewIngr_Crude { get; set; }

    public double? BrewIngr_Quantity { get; set; }

    public virtual BrewingBatch? BrewIngr_BrewBatchNavigation { get; set; }

    public virtual Crude? BrewIngr_CrudeNavigation { get; set; }
}
