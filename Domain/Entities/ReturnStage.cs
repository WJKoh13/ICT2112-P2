using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class ReturnStage
{
    public int StageId { get; set; }

    public int ReturnId { get; set; }

    public double? EnergyKwh { get; set; }

    public double? LabourHours { get; set; }

    public double? MaterialsKg { get; set; }

    public double? CleaningSuppliesQty { get; set; }

    public double? WaterLitres { get; set; }

    public double? PackagingKg { get; set; }

    public double? StorageHours { get; set; }

    public virtual ICollection<CarbonEmission> CarbonEmissions { get; set; } = new List<CarbonEmission>();

    public virtual ProductReturn Return { get; set; } = null!;
}
