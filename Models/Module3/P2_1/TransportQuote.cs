using ProRental.Domain.Enums;

namespace ProRental.Models.Module3.P2_1;

/// <summary>
/// Quote payload returned by the transport-carbon dependency to populate a ShippingOption.
/// by: ernest
/// </summary>
public sealed record TransportQuote(
    decimal Cost,
    double CarbonFootprintKg,
    int DeliveryDays,
    TransportMode TransportMode,
    string DisplayName,
    string TransportModeLabel);
