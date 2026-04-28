using System;
using System.Collections.Generic;

namespace BeerZdec.Models;

public partial class Supplier
{
    public int Supplier_ID { get; set; }

    public string? Supplier_Name { get; set; }

    public int? Supplier_INN { get; set; }

    public virtual ICollection<CrudeSupply> CrudeSupplies { get; set; } = new List<CrudeSupply>();
}
