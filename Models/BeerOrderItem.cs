using System;
using System.Collections.Generic;

namespace BeerZdec.Models;

public partial class BeerOrderItem
{
    public int BeerOrderItem_ID { get; set; }

    public int? BeerOrderItem_Order { get; set; }

    public int? BeerOrderItem_PackBatch { get; set; }

    public int? BeerOrderItem_UnitsQuantity { get; set; }

    public decimal? BeerOrderItem_PricePerUnit { get; set; }

    public virtual BeerOrder? BeerOrderItem_OrderNavigation { get; set; }

    public virtual PackagingBatch? BeerOrderItem_PackBatchNavigation { get; set; }
}
