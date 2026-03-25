using ProRental.Domain.Enums;

namespace ProRental.Domain.Entities;

public partial class DeliveryBatch
{
    private readonly List<int> _listOfOrders = [];

    private BatchStatus? _deliveryBatchStatus;
    private BatchStatus? DeliveryBatchStatus { get => _deliveryBatchStatus; set => _deliveryBatchStatus = value; }
    public void UpdateDeliveryBatchStatus(BatchStatus newValue) => _deliveryBatchStatus = newValue;

    public void InitializeNewBatch(int sourceHubId, string destinationAddress)
    {
        _hubId = sourceHubId;
        _destinationAddress = destinationAddress;
        _deliveryBatchStatus = BatchStatus.PENDING;
        _totalOrders = 0;
        _carbonSavings = 0d;
        _batchWeightKg = 0d;
    }

    public string GetSourceHub() => (_hubId ?? 0).ToString();
    public void SetSourceHub(string sourceHub)
    {
        if (int.TryParse(sourceHub, out var hubId))
        {
            _hubId = hubId;
        }
    }

    public string GetDestinationAddress() => _destinationAddress ?? string.Empty;
    public void SetDestinationAddress(string destinationAddress) => _destinationAddress = destinationAddress;

    public int GetDeliveryBatchIdentifier() => _deliveryBatchId;
    public BatchStatus? GetDeliveryBatchStatus() => _deliveryBatchStatus;
    public void SetDeliveryBatchStatus(BatchStatus status) => _deliveryBatchStatus = status;

    public int GetTotalOrders() => _totalOrders ?? 0;
    public void SetTotalOrders(int totalOrders) => _totalOrders = totalOrders;

    public double GetCarbonSavings() => _carbonSavings ?? 0d;
    public void SetCarbonSavings(double carbonSavings) => _carbonSavings = carbonSavings;

    public IReadOnlyList<int> GetListOfOrders() => _listOfOrders.ToArray();

    public double GetBatchWeightKG() => _batchWeightKg ?? 0d;
    public void SetBatchWeightKG(double weightKG) => _batchWeightKg = weightKG;

    public bool addOrder(int orderId)
    {
        if (_listOfOrders.Contains(orderId))
        {
            return false;
        }

        _listOfOrders.Add(orderId);
        _totalOrders = _listOfOrders.Count;
        return true;
    }

    public bool removeOrder(int orderId)
    {
        if (!_listOfOrders.Contains(orderId))
        {
            return false;
        }

        _listOfOrders.Remove(orderId);
        _totalOrders = _listOfOrders.Count;
        return true;
    }

    public void markAsShipped() => _deliveryBatchStatus = BatchStatus.SHIPPEDOUT;
    public void updateCarbonSavings(double carbonSavings) => _carbonSavings = carbonSavings;
    public void updateBatchWeight(double weightKg) => _batchWeightKg = weightKg;
}
