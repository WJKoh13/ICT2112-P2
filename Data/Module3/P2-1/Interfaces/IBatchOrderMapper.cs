namespace ProRental.Data.Module3.P2_1.Interfaces;

public interface IBatchOrderMapper
{
    Task<bool> AddOrderToBatch(int batchId, int orderId, CancellationToken cancellationToken = default);
    Task<bool> RemoveOrderFromBatch(int batchId, int orderId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<int>> GetOrderIdsByBatch(int batchId, CancellationToken cancellationToken = default);
    Task<int?> GetBatchIdByOrder(int orderId, CancellationToken cancellationToken = default);
    Task<bool> OrderAlreadyAssigned(int orderId, CancellationToken cancellationToken = default);
    Task<int> CountOrdersInBatch(int batchId, CancellationToken cancellationToken = default);
}
