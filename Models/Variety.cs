using System;
using System.Collections.Generic;

namespace BeerZdec.Models;

public partial class Variety
{
    public int Variety_ID { get; set; }

    public int Variety_Grain { get; set; }

    public string? Variety_NameRu { get; set; }

    public string? Variety_NameLatin { get; set; }

    public string? Variety_GosRegNum { get; set; }

    public string? Variety_MaturityGroup { get; set; }

    public string? Variety_MaltingPurpose { get; set; }

    public string? Variety_RowType { get; set; }

    public string? Variety_SeasonType { get; set; }

    public double? Variety_ExtrPotentMax { get; set; }

    public double? Variety_ExtrPotentMin { get; set; }

    public double? Variety_ProteinContMax { get; set; }

    public double? Variety_ProtContMin { get; set; }

    public int? Variety_ShelfLifeMonths { get; set; }

    public virtual Grain Variety_GrainNavigation { get; set; } = null!;
}
