using ProRental.Data.Module3.P2_1.Interfaces;
using ProRental.Interfaces.Module3.P2_1;

namespace ProRental.Domain.Module3.P2_1.Controls;

public sealed class BatchValidator : IBatchValidator
{
    private readonly IOrderService _orderService;
    private readonly IDeliveryBatchMapper _deliveryBatchMapper;

    public BatchValidator(IOrderService orderService, IDeliveryBatchMapper deliveryBatchMapper)
    {
        _orderService = orderService;
        _deliveryBatchMapper = deliveryBatchMapper;
    }

    public bool validateOrderExists(string orderId)
    {
        if (!int.TryParse(orderId, out var parsedOrderId))
        {
            return false;
        }

        try
        {
            return _orderService.GetShippingContextAsync(parsedOrderId).GetAwaiter().GetResult() is not null;
        }
        catch
        {
            return false;
        }
    }

    public bool validateBatchExists(int batchId)
    {
        return _deliveryBatchMapper.findByIdentifier(batchId.ToString()) is not null;
    }
}
