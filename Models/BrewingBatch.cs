using System;
using System.Collections.Generic;

namespace BeerZdec.Models;

public partial class BrewingBatch
{
    public int BrewBatch_ID { get; set; }

    public int? BrewBatch_BeerStyle { get; set; }

    public int? BrewBatch_Fermenter { get; set; }

    public int? BrewBatch_ConditionalTank { get; set; }

    public int? BrewBatch_Technologist { get; set; }

    public string BrewBatch_Code { get; set; } = null!;

    public DateOnly? BrewBatch_Datetime { get; set; }

    public double? BrewBatch_Volume { get; set; }

    public double? BrewBatch_ActualOG { get; set; }

    public double? BrewBatch_ActualFG { get; set; }

    public double? BrewBatch_ActualABV { get; set; }

    public int? BrewBatch_ActualIBU { get; set; }

    public int? BrewBatch_ActualColorEBC { get; set; }

    public string? BrewBatch_Status { get; set; }

    public virtual BeerStyle? BrewBatch_BeerStyleNavigation { get; set; }

    public virtual BrewEquipment? BrewBatch_ConditionalTankNavigation { get; set; }

    public virtual BrewEquipment? BrewBatch_FermenterNavigation { get; set; }

    public virtual Employee? BrewBatch_TechnologistNavigation { get; set; }

    public virtual ICollection<BrewIngredient> BrewIngredients { get; set; } = new List<BrewIngredient>();

    public virtual ICollection<PackagingBatch> PackagingBatches { get; set; } = new List<PackagingBatch>();
}
