namespace ProRental.Models.Module3.P2_1;

public sealed record BatchDisplayItem(
    int BatchId,
    int? HubId,
    string DestinationAddress,
    string Status,
    int TotalOrders,
    double BatchWeightKg,
    double CarbonSavingsKg,
    string OrderIds);
