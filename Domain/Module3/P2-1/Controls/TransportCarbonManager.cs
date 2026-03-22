using ProRental.Data.Module3.P2_1.Interfaces;
using ProRental.Domain.Enums;
using ProRental.Interfaces.Module3.P2_1;

namespace ProRental.Domain.Module3.P2_1.Controls;

public class TransportCarbonManager : ITransportCarbonService
{
    private readonly IPricingRuleGateway _pricingRuleGateway;
    private readonly IShippingOptionService _shippingOptionService;

    public TransportCarbonManager(
        IPricingRuleGateway pricingRuleGateway,
        IShippingOptionService shippingOptionService)
    {
        _pricingRuleGateway = pricingRuleGateway;
        _shippingOptionService = shippingOptionService;
    }

    public double CalculateForRoute(int shippingOptionId, double weightKg)
    {
        var (transportModes, distancesKm) = _shippingOptionService.GetRouteCarbonInputs(shippingOptionId);

        return transportModes.Zip(
                distancesKm,
                (mode, distanceKm) => CalculateLegCarbon(mode, distanceKm, weightKg))
            .Sum();
    }

    public double CalculateLegCarbon(TransportMode mode, double distanceKm, double weightKg)
    {
        var rule = _pricingRuleGateway.FindByTransportMode(mode)
            .FirstOrDefault(r => r.ReadIsActive());

        var baseRate = (double)(rule?.ReadBaseRatePerKm() ?? 0m);
        return weightKg * distanceKm * baseRate;
    }

    public float CalculateProductStorageCarbon(string productId, string hubId)
    {
        return 0f;
    }

    public double CalculateTotalCarbon(int quantity, double weightKg, double distanceKm, double baseRate, double storageCo2)
    {
        return (quantity * weightKg * distanceKm * baseRate) + storageCo2;
    }
}
