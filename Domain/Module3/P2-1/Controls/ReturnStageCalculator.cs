using ProRental.Data.Module3.P2_1.Interfaces;
using ProRental.Domain.Entities;

namespace ProRental.Domain.Module3.P2_1.Controls;

/// <summary>
/// Calculates carbon emissions (kg CO2) for each resource type in a return stage.
/// Emission factors sourced from standard industry averages.
/// </summary>
public class ReturnStageCalculator
{
    // kg CO2 per kWh (UK/SG grid average)
    private const double EnergyEmissionFactor = 0.233;
    // kg CO2 per labour-hour (commute + operational overhead)
    private const double LabourEmissionFactor = 0.3;
    // kg CO2 per kg of materials processed
    private const double MaterialsEmissionFactor = 2.0;
    // kg CO2 per litre of water
    private const double WaterEmissionFactor = 0.000344;
    // kg CO2 per unit of cleaning supplies
    private const double CleaningSuppliesEmissionFactor = 0.5;
    // kg CO2 per kg of packaging
    private const double PackagingEmissionFactor = 1.5;

    private readonly IReturnStageGateway _gateway;

    public ReturnStageCalculator(IReturnStageGateway gateway)
    {
        _gateway = gateway;
    }

    public double CalculateEnergyCarbon(int stageId)
    {
        var stage = _gateway.FindById(stageId);
        if (stage == null) return 0;
        return stage.GetEnergyKwh() * EnergyEmissionFactor;
    }

    public double CalculateLabourCarbon(int stageId)
    {
        var stage = _gateway.FindById(stageId);
        if (stage == null) return 0;
        return stage.GetLabourHours() * LabourEmissionFactor;
    }

    public double CalculateMaterialsCarbon(int stageId)
    {
        var stage = _gateway.FindById(stageId);
        if (stage == null) return 0;
        return stage.GetMaterialsKg() * MaterialsEmissionFactor;
    }

    public double CalculateWaterCarbon(int stageId)
    {
        var stage = _gateway.FindById(stageId);
        if (stage == null) return 0;
        return stage.GetWaterLitres() * WaterEmissionFactor;
    }

    public double CalculateCleaningSuppliesCarbon(int stageId)
    {
        var stage = _gateway.FindById(stageId);
        if (stage == null) return 0;
        return stage.GetCleaningSuppliesQty() * CleaningSuppliesEmissionFactor;
    }

    public double CalculatePackagingCarbon(int stageId)
    {
        var stage = _gateway.FindById(stageId);
        if (stage == null) return 0;
        return stage.GetPackagingKg() * PackagingEmissionFactor;
    }

    public double CalculateStageCarbon(int stageId)
    {
        return CalculateEnergyCarbon(stageId)
            + CalculateLabourCarbon(stageId)
            + CalculateMaterialsCarbon(stageId)
            + CalculateWaterCarbon(stageId)
            + CalculateCleaningSuppliesCarbon(stageId)
            + CalculatePackagingCarbon(stageId);
    }

    public double CalculateTotalCarbon(int returnRequestId)
    {
        var stages = _gateway.FindByReturnId(returnRequestId);
        return stages.Sum(s => CalculateStageCarbon(s.GetStageId()));
    }

    // Used internally by other services to calculate carbon for an already-loaded stage
    internal double CalculateStageCarbonFromEntity(ReturnStage stage)
    {
        return (stage.GetEnergyKwh() * EnergyEmissionFactor)
            + (stage.GetLabourHours() * LabourEmissionFactor)
            + (stage.GetMaterialsKg() * MaterialsEmissionFactor)
            + (stage.GetWaterLitres() * WaterEmissionFactor)
            + (stage.GetCleaningSuppliesQty() * CleaningSuppliesEmissionFactor)
            + (stage.GetPackagingKg() * PackagingEmissionFactor);
    }
}
