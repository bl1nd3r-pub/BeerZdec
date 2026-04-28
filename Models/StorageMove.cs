using System;
using System.Collections.Generic;

namespace BeerZdec.Models;

public partial class StorageMove
{
    public int StorageMoves_Zapis_ID { get; set; }

    public int? StorageMoves_GrainBatch { get; set; }

    public int? StorageMoves_FromStorage { get; set; }

    public int? StorageMoves_ToStorage { get; set; }

    public int? StorageMoves_MovedBy { get; set; }

    public double? ElevMoves_Weight { get; set; }

    public virtual StorageCell? StorageMoves_FromStorageNavigation { get; set; }

    public virtual GrainBatch? StorageMoves_GrainBatchNavigation { get; set; }

    public virtual Employee? StorageMoves_MovedByNavigation { get; set; }

    public virtual StorageCell? StorageMoves_ToStorageNavigation { get; set; }
}
