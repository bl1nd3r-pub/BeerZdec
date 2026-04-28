using System;
using System.Collections.Generic;

namespace BeerZdec.Models;

public partial class MaltProcess
{
    public int MaltProcess_ID { get; set; }

    public int MaltProcess_MaltOrder { get; set; }

    public int MaltProcess_MaltLine { get; set; }

    public int MaltProcess_Technologist { get; set; }

    public DateTime? MaltProcess_StartTime { get; set; }

    public DateTime? MaltProcess_EndTime { get; set; }

    public virtual ICollection<MaltBatch> MaltBatches { get; set; } = new List<MaltBatch>();

    public virtual MaltingLine MaltProcess_MaltLineNavigation { get; set; } = null!;

    public virtual MaltingOrder MaltProcess_MaltOrderNavigation { get; set; } = null!;

    public virtual Employee MaltProcess_TechnologistNavigation { get; set; } = null!;
}
