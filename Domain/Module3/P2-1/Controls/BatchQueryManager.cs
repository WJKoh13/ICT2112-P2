using ProRental.Data.Module3.P2_1.Interfaces;
using ProRental.Domain.Entities;
using ProRental.Interfaces.Module3.P2_1;

namespace ProRental.Domain.Module3.P2_1.Controls;

public sealed class BatchQueryManager : IBatchDisplayManager, IBatchQueryManager
{
    private readonly IDeliveryBatchMapper _deliveryBatchMapper;

    public BatchQueryManager(IDeliveryBatchMapper deliveryBatchMapper)
    {
        _deliveryBatchMapper = deliveryBatchMapper;
    }

    public List<string> getBatches()
    {
        return _deliveryBatchMapper
            .findAll()
            .Select(entity =>
                $"Batch #{entity.GetDeliveryBatchIdentifier()} | Hub: {entity.GetSourceHub()} | " +
                $"Orders: {entity.GetTotalOrders()} | Savings: {entity.GetCarbonSavings():0.##} kg")
            .ToList();
    }

    public List<DeliveryBatch> GetBatchesForDisplay()
    {
        return _deliveryBatchMapper.findAll();
    }
}
