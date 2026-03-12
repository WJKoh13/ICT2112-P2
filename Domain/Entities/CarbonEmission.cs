using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class CarbonEmission
{
    public int EmissionId { get; set; }

    public int StageId { get; set; }

    public double? CarbonKg { get; set; }

    public virtual ReturnStage Stage { get; set; } = null!;
}
