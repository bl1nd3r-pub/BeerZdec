using System;
using System.Collections.Generic;

namespace BeerZdec.Models;

public partial class CrudeSupply
{
    public int CrudeSupply_ID { get; set; }

    public int? CrudeSupply_Crude { get; set; }

    public int? CrudeSupply_Supplier { get; set; }

    public DateTime? CrudeSupply_Datetime { get; set; }

    public double? CrudeSupply_Amount { get; set; }

    public virtual SuppliableCrude? CrudeSupply_CrudeNavigation { get; set; }

    public virtual Supplier? CrudeSupply_SupplierNavigation { get; set; }

    public virtual ICollection<Crude> Crudes { get; set; } = new List<Crude>();
}
