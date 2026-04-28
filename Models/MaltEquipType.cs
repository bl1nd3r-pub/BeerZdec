using System;
using System.Collections.Generic;

namespace BeerZdec.Models;

public partial class MaltEquipType
{
    public int MaltEquipType_ID { get; set; }

    public string? MaltEquipType_Name { get; set; }

    public string? MaltEquipType_Description { get; set; }

    public virtual ICollection<MaltEquipment> MaltEquipments { get; set; } = new List<MaltEquipment>();
}
