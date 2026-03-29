namespace ProRental.Data.Module3.P2_1.Interfaces;

public interface IBatchOrderMapper
{
    bool addOrderToBatch(int batchId, string orderId);
    bool removeOrderFromBatch(string batchId, string orderId);
    List<string> getOrderIdsByBatch(int batchId);
    string? getBatchIdByOrder(string orderId);
    bool orderAlreadyAssigned(string orderId);
    int countOrdersInBatch(int batchId);
}
