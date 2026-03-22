using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class CarbonEmission
{
    private int _emissionId;
    public int EmissionId { get => _emissionId; set => _emissionId = value; }

    private int _stageId;
    public int StageId { get => _stageId; set => _stageId = value; }

    private double? _carbonKg;
    public double? CarbonKg { get => _carbonKg; set => _carbonKg = value; }

    public virtual ReturnStage Stage { get; private set; } = null!;
}
