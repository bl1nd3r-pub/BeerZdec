using System;
using System.Collections.Generic;

namespace BeerZdec.Models;

public partial class PackagingBatch
{
    public int PackBatch_ID { get; set; }

    public int? PackBatch_BrewBatch { get; set; }

    public DateTime? PackBatch_Datetime { get; set; }

    public string? PackBatch_Type { get; set; }

    public double? PackBatch_Volume { get; set; }

    public int? PackBatch_UnitsCount { get; set; }

    public int? PackBatch_ShelfLife { get; set; }

    public string? PackBatch_Status { get; set; }

    public virtual ICollection<BeerOrderItem> BeerOrderItems { get; set; } = new List<BeerOrderItem>();

    public virtual BrewingBatch? PackBatch_BrewBatchNavigation { get; set; }
}
