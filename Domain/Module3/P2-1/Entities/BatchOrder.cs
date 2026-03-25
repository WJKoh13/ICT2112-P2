namespace ProRental.Domain.Entities;

public partial class BatchOrder
{
    public int GetBatchId() => _batchId;
    public int GetOrderId() => _orderId;

    public void Assign(int batchId, int orderId)
    {
        _batchId = batchId;
        _orderId = orderId;
        _addedTimestamp = DateTime.UtcNow;
    }
}
