using System;
using System.Collections.Generic;

namespace BeerZdec.Models;

public partial class StorageCell
{
    public int Storage_ID { get; set; }

    public double? Storage_MaxCapacity { get; set; }

    public double? Storage_CurOccup { get; set; }

    public double? Storage_Condition { get; set; }

    public virtual ICollection<StorageMove> StorageMoveStorageMoves_FromStorageNavigations { get; set; } = new List<StorageMove>();

    public virtual ICollection<StorageMove> StorageMoveStorageMoves_ToStorageNavigations { get; set; } = new List<StorageMove>();

    public virtual ICollection<StorageToMalting> StorageToMaltings { get; set; } = new List<StorageToMalting>();
}
