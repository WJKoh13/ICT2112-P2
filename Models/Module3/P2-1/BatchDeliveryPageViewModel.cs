namespace ProRental.Models.Module3.P2_1;

public sealed class BatchDeliveryPageViewModel
{
    public IReadOnlyList<BatchDisplayItem> Batches { get; init; } = [];
    public double TotalCarbonSavingsKg { get; init; }
}
