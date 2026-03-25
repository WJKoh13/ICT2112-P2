using ProRental.Data.Module3.P2_1.Interfaces;
using ProRental.Interfaces.Module3.P2_1;

namespace ProRental.Domain.Module3.P2_1.Controls;

public sealed class BatchValidator : IBatchValidator
{
    private readonly IDeliveryBatchMapper _deliveryBatchMapper;
    private readonly IOrderService _orderService;

    public BatchValidator(IDeliveryBatchMapper deliveryBatchMapper, IOrderService orderService)
    {
        _deliveryBatchMapper = deliveryBatchMapper;
        _orderService = orderService;
    }

    public async Task<bool> ValidateOrderExists(int orderId, CancellationToken cancellationToken = default)
    {
        var address = await _orderService.GetDeliveryAddress(orderId, cancellationToken);
        return !string.IsNullOrWhiteSpace(address);
    }

    public async Task<bool> ValidateBatchExists(int batchId, CancellationToken cancellationToken = default)
    {
        var batch = await _deliveryBatchMapper.FindByIdentifier(batchId, cancellationToken);
        return batch is not null;
    }
}
