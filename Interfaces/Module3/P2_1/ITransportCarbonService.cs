using ProRental.Domain.Entities;
using ProRental.Domain.Enums;
using ProRental.Models.Module3.P2_1;

namespace ProRental.Interfaces.Module3.P2_1;

/// <summary>
/// Feature 2 quote contract consumed by Feature 1 to obtain checkout-facing cost and carbon values.
/// by: ernest
/// </summary>
public interface ITransportCarbonService
{
    Task<TransportQuote> QuoteAsync(
        DeliveryRoute route,
        OrderShippingContext context,
        PreferenceType preferenceType,
        CancellationToken cancellationToken = default);
}
