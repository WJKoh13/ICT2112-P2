using ProRental.Domain.Enums;

namespace ProRental.Domain.Entities;

public partial class DeliveryBatch
{
    private BatchStatus? _deliveryBatchStatus;
    private BatchStatus? DeliveryBatchStatus { get => _deliveryBatchStatus; set => _deliveryBatchStatus = value; }
    public void UpdateDeliveryBatchStatus(BatchStatus newValue) => _deliveryBatchStatus = newValue;

    public int GetDeliveryBatchIdentifier() => _deliveryBatchId;
    public int? GetSourceHub() => _hubId;
    public string GetDestinationAddress() => _destinationAddress ?? string.Empty;
    public BatchStatus GetDeliveryBatchStatus() => _deliveryBatchStatus ?? BatchStatus.PENDING;
    public int GetTotalOrders() => _totalOrders ?? 0;
    public double GetCarbonSavings() => _carbonSavings ?? 0d;
    public double GetBatchWeightKg() => _batchWeightKg ?? 0d;
    public IReadOnlyList<int> GetListOfOrders() => BatchOrders.Select(item => item.GetOrderId()).ToList();

    public void InitializeBatch(int hubId, string destinationAddress)
    {
        _hubId = hubId;
        _destinationAddress = destinationAddress;
        _totalOrders = 0;
        _carbonSavings = 0d;
        _batchWeightKg = 0d;
        _deliveryBatchStatus = BatchStatus.PENDING;
    }

    public bool AddOrder(int orderId)
    {
        if (BatchOrders.Any(item => item.GetOrderId() == orderId))
        {
            return false;
        }

        var batchOrder = new BatchOrder();
        batchOrder.Assign(_deliveryBatchId, orderId);
        BatchOrders.Add(batchOrder);
        _totalOrders = BatchOrders.Count;

        return true;
    }

    public bool RemoveOrder(int orderId)
    {
        var existing = BatchOrders.FirstOrDefault(item => item.GetOrderId() == orderId);
        if (existing is null)
        {
            return false;
        }

        BatchOrders.Remove(existing);
        _totalOrders = BatchOrders.Count;
        return true;
    }

    public void MarkAsShipped() => _deliveryBatchStatus = BatchStatus.SHIPPEDOUT;
    public void UpdateCarbonSavings(double carbonSavings) => _carbonSavings = carbonSavings;
    public void UpdateBatchWeight(double weightKg) => _batchWeightKg = weightKg;
}

