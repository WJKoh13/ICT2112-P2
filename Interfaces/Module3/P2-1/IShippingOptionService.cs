using ProRental.Domain.Enums;

namespace ProRental.Interfaces.Module3.P2_1;

public interface IShippingOptionService
{
    (IEnumerable<TransportMode> TransportModes, IEnumerable<double> DistancesKm) GetRouteCarbonInputs(int shippingOptionId);
}
