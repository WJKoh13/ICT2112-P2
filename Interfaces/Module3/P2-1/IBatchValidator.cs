namespace ProRental.Interfaces.Module3.P2_1;

public interface IBatchValidator
{
    Task<bool> ValidateOrderExists(int orderId, CancellationToken cancellationToken = default);
    Task<bool> ValidateBatchExists(int batchId, CancellationToken cancellationToken = default);
}
