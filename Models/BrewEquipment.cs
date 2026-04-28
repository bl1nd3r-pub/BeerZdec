using System;
using System.Collections.Generic;

namespace BeerZdec.Models;

public partial class BrewEquipment
{
    public int BrewEquipment_ID { get; set; }

    public int? BrewEquipment_Type { get; set; }

    public double? BrewEquipment_Capacity { get; set; }

    public string? BrewEquipment_MeasUnit { get; set; }

    public bool? BrewEquipment_isActive { get; set; }

    public DateOnly? BrewEquipment_InstallDate { get; set; }

    public string? BrewEquipment_Code { get; set; }

    public virtual BrewEqType? BrewEquipment_TypeNavigation { get; set; }

    public virtual ICollection<BrewingBatch> BrewingBatchBrewBatch_ConditionalTankNavigations { get; set; } = new List<BrewingBatch>();

    public virtual ICollection<BrewingBatch> BrewingBatchBrewBatch_FermenterNavigations { get; set; } = new List<BrewingBatch>();
}
