using ProRental.Domain.Enums;

namespace ProRental.Models.Module3.P2_1;

/// <summary>
/// Read model returned from the persisted checkout shipping option.
/// by: ernest
/// </summary>
public sealed record ShippingOptionSummary(
    int OptionId,
    int CheckoutId,
    PreferenceType PreferenceType,
    string DisplayName,
    decimal Cost,
    double CarbonFootprintKg,
    int DeliveryDays,
    int? RouteId,
    TransportMode? TransportMode,
    string TransportModeLabel);
