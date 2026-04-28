using System;
using System.Collections.Generic;

namespace BeerZdec.Models;

public partial class GrainBatch
{
    public int GB_ID { get; set; }

    public int GB_Harvest { get; set; }

    public int GB_Status { get; set; }

    public int GB_QualGrade { get; set; }

    public double? GB_Moisture { get; set; }

    public double? GB_ForeignMatter { get; set; }

    public double? GB_WeightReceived { get; set; }

    public virtual HarvestEvent GB_HarvestNavigation { get; set; } = null!;

    public virtual GBQualGrade GB_QualGradeNavigation { get; set; } = null!;

    public virtual GBStatus GB_StatusNavigation { get; set; } = null!;

    public virtual ICollection<StorageMove> StorageMoves { get; set; } = new List<StorageMove>();
}
