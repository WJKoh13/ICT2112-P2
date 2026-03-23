using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class ReturnStage
{
    private int _stageId;
    public int StageId { get => _stageId; set => _stageId = value; }

    private int _returnId;
    public int ReturnId { get => _returnId; set => _returnId = value; }

    private double? _energyKwh;
    public double? EnergyKwh { get => _energyKwh; set => _energyKwh = value; }

    private double? _labourHours;
    public double? LabourHours { get => _labourHours; set => _labourHours = value; }

    private double? _materialsKg;
    public double? MaterialsKg { get => _materialsKg; set => _materialsKg = value; }

    private double? _cleaningSuppliesQty;
    public double? CleaningSuppliesQty { get => _cleaningSuppliesQty; set => _cleaningSuppliesQty = value; }

    private double? _waterLitres;
    public double? WaterLitres { get => _waterLitres; set => _waterLitres = value; }

    private double? _packagingKg;
    public double? PackagingKg { get => _packagingKg; set => _packagingKg = value; }

    private decimal? _surchargeRate;
    public decimal? SurchargeRate { get => _surchargeRate; set => _surchargeRate = value; }

    public virtual ICollection<CarbonEmission> CarbonEmissions { get; private set; } = new List<CarbonEmission>();

    public virtual Returnrequest Return { get; private set; } = null!;
}
