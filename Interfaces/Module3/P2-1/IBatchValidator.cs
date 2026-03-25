namespace ProRental.Interfaces.Module3.P2_1;

public interface IBatchValidator
{
    bool validateOrderExists(string orderId);
    bool validateBatchExists(int batchId);
}
