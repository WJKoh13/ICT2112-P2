namespace ProRental.Interfaces.Module3.P2_1;

/// <summary>
/// Shared Feature 2 transport-carbon contract consumed by both the transport-carbon flow
/// and Feature 1's shipping-option generation logic.
/// by: ernest
/// </summary>
public interface ITransportCarbonService
{
    double CalculateLegCarbon(int quantity, double weightKg, double distanceKm, double storageCo2);
    double CalculateRouteCarbon(IReadOnlyList<double> legCarbonValues);
    double CalculateCarbonSurcharge(double totalCarbonFootprint, double surchargeRate);
}
