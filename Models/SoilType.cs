using System;
using System.Collections.Generic;

namespace BeerZdec.Models;

public partial class SoilType
{
    public int SoilType_ID { get; set; }

    public int SoilType_TextureClass { get; set; }

    public string SoilType_Name { get; set; } = null!;

    public double? SoilType_FieldCapacity { get; set; }

    public double? SoilType_InfiltrationRate { get; set; }

    public double? SoilType_WiltingPoint { get; set; }

    public double? SoilType_PhMin { get; set; }

    public double? SoilType_PhMax { get; set; }

    public virtual SoilTextureClass SoilType_TextureClassNavigation { get; set; } = null!;

    public virtual ICollection<SowingPlot> SowingPlots { get; set; } = new List<SowingPlot>();
}
