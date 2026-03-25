namespace ProRental.Interfaces.Module3.P2_1;

public interface IBatchDelivery
{
    void consolidateOrderToBatch(string orderId);
    bool markBatchesAsShipped(List<string> batchIds);
}
