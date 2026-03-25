using ProRental.Domain.Entities;
using ProRental.Domain.Enums;

namespace ProRental.Data.Module3.P2_1.Interfaces;

public interface IDeliveryBatchMapper
{
    DeliveryBatch createNewBatch(int destHubID, string destinationAddress);
    DeliveryBatch? findByIdentifier(string batchIdentifier);
    List<DeliveryBatch> findAll();
    List<DeliveryBatch> getAllPendingBatches();
    List<DeliveryBatch> getBatchByStatus(string warehouse, BatchStatus status);
    bool insert(DeliveryBatch batch);
    bool update(DeliveryBatch batch);
    bool delete(string batchIdentifier);
}
