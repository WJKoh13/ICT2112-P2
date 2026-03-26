using ProRental.Data.Module3.P2_1.Interfaces;
using ProRental.Domain.Enums;

namespace ProRental.Domain.Module3.P2_1.Controls;

public record StageCarbonBreakdown(
    int StageId,
    CarbonStageType? StageType,
    double EnergyCarbon,
    double LabourCarbon,
    double MaterialsCarbon,
    double WaterCarbon,
    double CleaningSuppliesCarbon,
    double PackagingCarbon,
    double TotalCarbon,
    bool IsHighCarbon
)
{
    public string StageLabel => StageType?.ToString().Replace("_", " ") ?? "Unknown";
}

public record CarbonReport(
    int? ReturnRequestId,
    List<StageCarbonBreakdown> Stages,
    double TotalCarbonKg,
    decimal TotalSurcharge,
    List<StageCarbonBreakdown> HighCarbonStages,
    List<int> AvailableReturnIds
)
{
    public bool HasHighCarbonStages => HighCarbonStages.Count > 0;
    public int HighCarbonStageCount => HighCarbonStages.Count;
}

/// <summary>
/// Aggregates carbon data across all stages of a return request into a report.
/// </summary>
public class ReturnCarbonReportService
{
    // Stages exceeding this threshold (kg CO2) are flagged as high-carbon
    private const double HighCarbonThresholdKg = 10.0;

    private readonly IReturnStageGateway _gateway;
    private readonly ReturnStageCalculator _calculator;
    private readonly ReturnStageSurchargeService _surchargeService;

    public ReturnCarbonReportService(
        IReturnStageGateway gateway,
        ReturnStageCalculator calculator,
        ReturnStageSurchargeService surchargeService)
    {
        _gateway = gateway;
        _calculator = calculator;
        _surchargeService = surchargeService;
    }

    public CarbonReport GetCarbonReport(int returnRequestId)
    {
        var allStages = _gateway.FindAll();
        var availableIds = allStages.Select(s => s.GetReturnId()).Distinct().OrderBy(id => id).ToList();

        var stages = _gateway.FindByReturnId(returnRequestId);
        var breakdowns = BuildBreakdowns(stages);

        double totalCarbonKg = breakdowns.Sum(b => b.TotalCarbon);
        decimal totalSurcharge = _surchargeService.CalculateStageSurcharge(returnRequestId);
        var highCarbonStages = breakdowns.Where(b => b.IsHighCarbon).ToList();

        return new CarbonReport(
            ReturnRequestId: returnRequestId,
            Stages: breakdowns,
            TotalCarbonKg: totalCarbonKg,
            TotalSurcharge: totalSurcharge,
            HighCarbonStages: highCarbonStages,
            AvailableReturnIds: availableIds
        );
    }

    public CarbonReport GetAllCarbonReport()
    {
        var stages = _gateway.FindAll();
        var availableIds = stages.Select(s => s.GetReturnId()).Distinct().OrderBy(id => id).ToList();
        var breakdowns = BuildBreakdowns(stages);

        double totalCarbonKg = breakdowns.Sum(b => b.TotalCarbon);
        decimal totalSurcharge = stages
            .GroupBy(s => s.GetReturnId())
            .Sum(g => _surchargeService.CalculateStageSurcharge(g.Key));
        var highCarbonStages = breakdowns.Where(b => b.IsHighCarbon).ToList();

        return new CarbonReport(
            ReturnRequestId: null,
            Stages: breakdowns,
            TotalCarbonKg: totalCarbonKg,
            TotalSurcharge: totalSurcharge,
            HighCarbonStages: highCarbonStages,
            AvailableReturnIds: availableIds
        );
    }

    private List<StageCarbonBreakdown> BuildBreakdowns(List<ProRental.Domain.Entities.ReturnStage> stages)
    {
        return stages.Select(stage =>
        {
            int stageId = stage.GetStageId();
            double energy = _calculator.CalculateEnergyCarbon(stageId);
            double labour = _calculator.CalculateLabourCarbon(stageId);
            double materials = _calculator.CalculateMaterialsCarbon(stageId);
            double water = _calculator.CalculateWaterCarbon(stageId);
            double cleaning = _calculator.CalculateCleaningSuppliesCarbon(stageId);
            double packaging = _calculator.CalculatePackagingCarbon(stageId);
            double total = energy + labour + materials + water + cleaning + packaging;

            return new StageCarbonBreakdown(
                StageId: stageId,
                StageType: stage.GetStageType(),
                EnergyCarbon: energy,
                LabourCarbon: labour,
                MaterialsCarbon: materials,
                WaterCarbon: water,
                CleaningSuppliesCarbon: cleaning,
                PackagingCarbon: packaging,
                TotalCarbon: total,
                IsHighCarbon: total >= HighCarbonThresholdKg
            );
        }).ToList();
    }
}
