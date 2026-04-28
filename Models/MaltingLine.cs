using System;
using System.Collections.Generic;

namespace BeerZdec.Models;

public partial class MaltingLine
{
    public int MaltingLine_ID { get; set; }

    public string? MaltingLine_CurStatus { get; set; }

    public string? MaltingLine_LocationZone { get; set; }

    public double? MaltingLine_TotalCapacity { get; set; }

    public virtual ICollection<MaltEquipment> MaltEquipments { get; set; } = new List<MaltEquipment>();

    public virtual ICollection<MaltProcess> MaltProcesses { get; set; } = new List<MaltProcess>();
}
