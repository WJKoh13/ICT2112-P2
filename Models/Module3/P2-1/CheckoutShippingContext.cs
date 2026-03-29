namespace ProRental.Models.Module3.P2_1;

/// <summary>
/// Stable checkout input snapshot passed into Feature 1 option generation.
/// by: ernest
/// </summary>
public sealed record CheckoutShippingItem(
    int ProductId,
    int Quantity,
    double UnitWeightKg);

/// <summary>
/// Aggregate checkout input snapshot passed into Feature 1 option generation.
/// by: ernest
/// </summary>
public sealed record CheckoutShippingContext(
    int CheckoutId,
    int CustomerId,
    string DestinationAddress,
    int HubId,
    IReadOnlyList<CheckoutShippingItem> Items,
    double TotalShipmentWeightKg);
