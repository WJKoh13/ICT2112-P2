using ProRental.Data.Module3.P2_1.Interfaces;
using ProRental.Domain.Module3.P2_1.Interfaces;

namespace ProRental.Domain.Module3.P2_1.Controls;

/// <summary>
/// Calculates carbon surcharges (in dollars) for return requests.
/// The surcharge is the per-stage carbon (kg CO2) multiplied by the stage's surcharge rate.
/// </summary>
public class ReturnStageSurchargeService : ICarbonReturnService
{
    private readonly IReturnStageGateway _gateway;
    private readonly ReturnStageCalculator _calculator;

    public ReturnStageSurchargeService(IReturnStageGateway gateway, ReturnStageCalculator calculator)
    {
        _gateway = gateway;
        _calculator = calculator;
    }

    /// <summary>
    /// Returns the total surcharge across all stages for a given return request.
    /// </summary>
    public decimal CalculateStageSurcharge(int returnRequestId)
    {
        var stages = _gateway.FindByReturnId(returnRequestId);
        return stages.Sum(stage =>
        {
            var carbonKg = (decimal)_calculator.CalculateStageCarbonFromEntity(stage);
            return carbonKg * stage.GetSurchargeRate();
        });
    }

    /// <summary>
    /// Returns the carbon surcharge for a single stage.
    /// </summary>
    public decimal GetCarbonSurcharge(int stageId)
    {
        var stage = _gateway.FindById(stageId);
        if (stage == null) return 0m;

        var carbonKg = (decimal)_calculator.CalculateStageCarbonFromEntity(stage);
        return carbonKg * stage.GetSurchargeRate();
    }

    /// <summary>
    /// Handles carbon surcharge for a specific stage within a return request.
    /// </summary>
    public decimal HandleCarbonSurcharge(int returnId, int stageId)
    {
        var stage = _gateway.FindById(stageId);
        if (stage == null || stage.GetReturnId() != returnId) return 0m;

        var carbonKg = (decimal)_calculator.CalculateStageCarbonFromEntity(stage);
        return carbonKg * stage.GetSurchargeRate();
    }
}
