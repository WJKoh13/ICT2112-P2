using ProRental.Interfaces.Module3.P2_1;

namespace ProRental.Domain.Module3.P2_1.Controls;

/// <summary>
/// Shared Feature 2 transport-carbon implementation kept on the original calculation
/// contract so other features can compose their own workflows around it.
/// by: bryan
/// </summary>
public sealed class TransportCarbonManager : ITransportCarbonService
{
    public double CalculateLegCarbon(int quantity, double weightKg, double distanceKm, double storageCo2)
    {
        return (quantity * weightKg * distanceKm) + storageCo2;
    }

    public double CalculateRouteCarbon(IReadOnlyList<double> legCarbonValues)
    {
        return legCarbonValues.Sum();
    }

    public double CalculateCarbonSurcharge(double totalCarbonFootprint, double surchargeRate)
    {
        return totalCarbonFootprint * surchargeRate;
    }
}
