using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class ReturnStage
{
    private int _stageId;
    private int StageId { get => _stageId; set => _stageId = value; }

    private int _returnId;
    private int ReturnId { get => _returnId; set => _returnId = value; }

    private double? _energyKwh;
    private double? EnergyKwh { get => _energyKwh; set => _energyKwh = value; }

    private double? _labourHours;
    private double? LabourHours { get => _labourHours; set => _labourHours = value; }

    private double? _materialsKg;
    private double? MaterialsKg { get => _materialsKg; set => _materialsKg = value; }

    private double? _cleaningSuppliesQty;
    private double? CleaningSuppliesQty { get => _cleaningSuppliesQty; set => _cleaningSuppliesQty = value; }

    private double? _waterLitres;
    private double? WaterLitres { get => _waterLitres; set => _waterLitres = value; }

    private double? _packagingKg;
    private double? PackagingKg { get => _packagingKg; set => _packagingKg = value; }

    private decimal? _surchargeRate;
    private decimal? SurchargeRate { get => _surchargeRate; set => _surchargeRate = value; }

    public virtual ICollection<CarbonEmission> CarbonEmissions { get; private set; } = new List<CarbonEmission>();

    public virtual Returnrequest Return { get; private set; } = null!;
}
