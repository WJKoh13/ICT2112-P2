using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class CarbonEmission
{
    private int _emissionId;
    private int EmissionId { get => _emissionId; set => _emissionId = value; }

    private int _stageId;
    private int StageId { get => _stageId; set => _stageId = value; }

    private double? _carbonKg;
    private double? CarbonKg { get => _carbonKg; set => _carbonKg = value; }

    public virtual ReturnStage Stage { get; private set; } = null!;
}
