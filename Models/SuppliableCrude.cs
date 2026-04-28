using System;
using System.Collections.Generic;

namespace BeerZdec.Models;

public partial class SuppliableCrude
{
    public int SuppliableCrude_ID { get; set; }

    public string? SuppliableCrude_Name { get; set; }

    public string? SuppliableCrude_MeasurementUnit { get; set; }

    public virtual ICollection<CrudeSupply> CrudeSupplies { get; set; } = new List<CrudeSupply>();
}
