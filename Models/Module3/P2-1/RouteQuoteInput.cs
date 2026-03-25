namespace ProRental.Models.Module3.P2_1;

/// <summary>
/// Product-level transport-carbon quote input for an order-level shipment.
/// by: ernest
/// </summary>
public sealed record RouteQuoteItem(
    int ProductId,
    int Quantity,
    double UnitWeightKg);

/// <summary>
/// Aggregate route-quote input shared between shipping-option generation and transport-carbon pricing.
/// by: ernest
/// </summary>
public sealed record RouteQuoteInput(
    int HubId,
    IReadOnlyList<RouteQuoteItem> Items);
