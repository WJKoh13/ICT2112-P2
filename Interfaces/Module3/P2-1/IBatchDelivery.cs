namespace ProRental.Interfaces.Module3.P2_1;

public interface IBatchDelivery
{
    Task ConsolidateOrderToBatch(string orderId, CancellationToken cancellationToken = default);
    Task<bool> MarkBatchesAsShipped(IReadOnlyList<string> batchIds, CancellationToken cancellationToken = default);
}
