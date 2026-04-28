using System;
using System.Collections.Generic;

namespace BeerZdec.Models;

public partial class MaltingOrder
{
    public int MaltingOrder_ID { get; set; }

    public DateTime? MaltingOrder_СreatedAt { get; set; }

    public string? MaltingOrder_Status { get; set; }

    public string? MaltingOrder_TargetMaltType { get; set; }

    public virtual ICollection<MaltProcess> MaltProcesses { get; set; } = new List<MaltProcess>();

    public virtual ICollection<StorageToMalting> StorageToMaltings { get; set; } = new List<StorageToMalting>();
}
