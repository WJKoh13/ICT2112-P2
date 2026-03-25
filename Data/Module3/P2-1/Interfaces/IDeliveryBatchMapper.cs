using ProRental.Domain.Entities;
using ProRental.Domain.Enums;

namespace ProRental.Data.Module3.P2_1.Interfaces;

public interface IDeliveryBatchMapper
{
    DeliveryBatch CreateNewBatch(int destinationHubId, string destinationAddress);
    Task<DeliveryBatch?> FindByIdentifier(int batchIdentifier, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DeliveryBatch>> FindAll(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DeliveryBatch>> GetAllPendingBatches(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DeliveryBatch>> GetBatchByStatus(int warehouseHubId, BatchStatus status, CancellationToken cancellationToken = default);
    Task<bool> Insert(DeliveryBatch batch, CancellationToken cancellationToken = default);
    Task<bool> Update(DeliveryBatch batch, CancellationToken cancellationToken = default);
    Task<bool> Delete(int batchIdentifier, CancellationToken cancellationToken = default);
}
