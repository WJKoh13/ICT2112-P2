using ProRental.Data.Module3.P2_1.Interfaces;
using ProRental.Domain.Entities;
using ProRental.Interfaces.Module3.P2_1;

namespace ProRental.Domain.Module3.P2_1.Controls;

public sealed class BatchQueryManager : IBatchDisplayManager, IBatchQueryManager
{
    private readonly IDeliveryBatchMapper _deliveryBatchMapper;

    public BatchQueryManager(IDeliveryBatchMapper deliveryBatchMapper)
    {
        _deliveryBatchMapper = deliveryBatchMapper;
    }

    public async Task<IReadOnlyList<string>> GetBatches(CancellationToken cancellationToken = default)
    {
        var batches = await _deliveryBatchMapper.FindAll(cancellationToken);
        return batches
            .Select(batch =>
                $"Batch #{batch.GetDeliveryBatchIdentifier()} | Hub: {batch.GetSourceHub()} | Orders: {batch.GetTotalOrders()} | Savings: {batch.GetCarbonSavings():0.##} kg")
            .ToList();
    }

    public async Task<IReadOnlyList<DeliveryBatch>> GetBatchesForDisplay(CancellationToken cancellationToken = default)
    {
        return await _deliveryBatchMapper.FindAll(cancellationToken);
    }
}
