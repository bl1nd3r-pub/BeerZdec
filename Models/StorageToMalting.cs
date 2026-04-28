using System;
using System.Collections.Generic;

namespace BeerZdec.Models;

public partial class StorageToMalting
{
    public int STM_Zapis_ID { get; set; }

    public int STM_MaltOrder { get; set; }

    public int STM_Storage { get; set; }

    public double? STM_Quantity { get; set; }

    public DateTime? STM_Datetime { get; set; }

    public virtual MaltingOrder STM_MaltOrderNavigation { get; set; } = null!;

    public virtual StorageCell STM_StorageNavigation { get; set; } = null!;
}
