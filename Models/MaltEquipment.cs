using System;
using System.Collections.Generic;

namespace BeerZdec.Models;

public partial class MaltEquipment
{
    public int MaltEquipment_ID { get; set; }

    public int MaltEquipment_Type { get; set; }

    public int MaltEquipment_MaltingLine { get; set; }

    public string? MaltEquipment_Manufacturer { get; set; }

    public DateOnly? MaltEquipment_InstallDate { get; set; }

    public bool? MaltEquipment_IsActive { get; set; }

    public virtual MaltingLine MaltEquipment_MaltingLineNavigation { get; set; } = null!;

    public virtual MaltEquipType MaltEquipment_TypeNavigation { get; set; } = null!;
}
