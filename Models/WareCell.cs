using System;
using System.Collections.Generic;

namespace BeerZdec.Models;

public partial class WareCell
{
    public int WareCell_ID { get; set; }

    public int? WareCell_Crude { get; set; }

    public double? WareCell_MaxCapacity { get; set; }

    public double? WareCell_CurOccup { get; set; }

    public string? WareCell_Condition { get; set; }

    public virtual Crude? WareCell_CrudeNavigation { get; set; }
}
