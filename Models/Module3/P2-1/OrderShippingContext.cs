namespace ProRental.Models.Module3.P2_1;

/// <summary>
/// Stable order input snapshot passed into shipping and batching flows.
/// by: ernest
/// </summary>
public sealed record OrderShippingItem(
    int ProductId,
    int Quantity,
    double UnitWeightKg);

/// <summary>
/// Aggregate order input snapshot passed into shipping and batching flows.
/// by: ernest
/// </summary>
public sealed record OrderShippingContext(
    int OrderId,
    int CustomerId,
    int CheckoutId,
    string DestinationAddress,
    int HubId,
    IReadOnlyList<OrderShippingItem> Items,
    double TotalShipmentWeightKg);
