using ProRental.Domain.Enums;

namespace ProRental.Models.Module3.P2_1;

public sealed class BatchRowViewModel
{
    public int BatchId { get; init; }
    public string DestinationAddress { get; init; } = string.Empty;
    public int HubId { get; init; }
    public BatchStatus? Status { get; init; }
    public int TotalOrders { get; init; }
    public double BatchWeightKg { get; init; }
    public double CarbonSavingsKg { get; init; }
    public List<int> OrderIds { get; init; } = [];
}

public sealed class BatchDeliveryPageViewModel
{
    public List<BatchRowViewModel> Batches { get; init; } = [];
    public string? Message { get; init; }
}

public sealed class BatchOrderAdminPageViewModel
{
    public string? Message { get; init; }
    public List<BatchRowViewModel> Batches { get; init; } = [];
}

public sealed class BatchOrderAdminFormModel
{
    public int OrderId { get; set; }
    public bool UseBatchShipping { get; set; }
    public string BatchIdsCsv { get; set; } = string.Empty;
}
