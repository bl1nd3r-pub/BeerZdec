using System;
using System.Collections.Generic;

namespace BeerZdec.Models;

public partial class Crude
{
    public int Crude_ID { get; set; }

    public int? Crude_MaltBatch { get; set; }

    public int? Crude_OtherBatch { get; set; }

    public virtual ICollection<BrewIngredient> BrewIngredients { get; set; } = new List<BrewIngredient>();

    public virtual MaltBatch? Crude_MaltBatchNavigation { get; set; }

    public virtual CrudeSupply? Crude_OtherBatchNavigation { get; set; }

    public virtual ICollection<WareCell> WareCells { get; set; } = new List<WareCell>();
}
