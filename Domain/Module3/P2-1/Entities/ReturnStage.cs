using ProRental.Domain.Enums;

namespace ProRental.Domain.Entities;

public partial class ReturnStage
{
    private CarbonStageType? _stageType;
    private CarbonStageType? StageType { get => _stageType; set => _stageType = value; }
    public void UpdateStageType(CarbonStageType newValue) => _stageType = newValue;

    private StageType? _stageTypeAlt;
    private StageType? StageTypeAlt { get => _stageTypeAlt; set => _stageTypeAlt = value; }
    public void UpdateStageTypeAlt(StageType newValue) => _stageTypeAlt = newValue;

    // Public getters for carbon resource fields
    public int GetStageId() => _stageId;
    public int GetReturnId() => _returnId;
    public double GetEnergyKwh() => _energyKwh ?? 0;
    public double GetLabourHours() => _labourHours ?? 0;
    public double GetMaterialsKg() => _materialsKg ?? 0;
    public double GetCleaningSuppliesQty() => _cleaningSuppliesQty ?? 0;
    public double GetWaterLitres() => _waterLitres ?? 0;
    public double GetPackagingKg() => _packagingKg ?? 0;
    public decimal GetSurchargeRate() => _surchargeRate ?? 0m;
    public CarbonStageType? GetStageType() => _stageType;

    // Setters for creating/updating stages
    public void SetReturnId(int returnId) => _returnId = returnId;
    public void SetEnergyKwh(double value) => _energyKwh = value;
    public void SetLabourHours(double value) => _labourHours = value;
    public void SetMaterialsKg(double value) => _materialsKg = value;
    public void SetCleaningSuppliesQty(double value) => _cleaningSuppliesQty = value;
    public void SetWaterLitres(double value) => _waterLitres = value;
    public void SetPackagingKg(double value) => _packagingKg = value;
    public void SetSurchargeRate(decimal value) => _surchargeRate = value;

    // Business methods
    public bool IsValidStage() =>
        _stageType.HasValue &&
        (_energyKwh >= 0 || _labourHours >= 0 || _materialsKg >= 0);

    public double GetTotalResourceCount() =>
        (_energyKwh ?? 0) +
        (_labourHours ?? 0) +
        (_materialsKg ?? 0) +
        (_cleaningSuppliesQty ?? 0) +
        (_waterLitres ?? 0) +
        (_packagingKg ?? 0);
}