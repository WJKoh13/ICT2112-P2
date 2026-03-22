using ProRental.Domain.Enums;

namespace ProRental.Interfaces.Module3.P2_1;

public interface ITransportCarbonService
{
    double CalculateForRoute(int shippingOptionId, double weightKg);
    double CalculateLegCarbon(TransportMode mode, double distanceKm, double weightKg);
    float CalculateProductStorageCarbon(string productId, string hubId);
    double CalculateTotalCarbon(int quantity, double weightKg, double distanceKm, double baseRate, double storageCo2);
}
