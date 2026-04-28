using System;
using System.Collections.Generic;

namespace BeerZdec.Models;

public partial class BrewEqType
{
    public int BrewEqType_ID { get; set; }

    public string BrewEqType_Name { get; set; } = null!;

    public virtual ICollection<BrewEquipment> BrewEquipments { get; set; } = new List<BrewEquipment>();
}
