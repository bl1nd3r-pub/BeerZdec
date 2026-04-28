using System;
using System.Collections.Generic;

namespace BeerZdec.Models;

public partial class SoilTextureClass
{
    public int SoilTextureClass_ID { get; set; }

    public string? SoilTextureClass_Name { get; set; }

    public virtual ICollection<SoilType> SoilTypes { get; set; } = new List<SoilType>();
}
