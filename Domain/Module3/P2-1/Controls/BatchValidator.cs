using Microsoft.EntityFrameworkCore;
using ProRental.Data.Module3.P2_1.Interfaces;
using ProRental.Data.UnitOfWork;
using ProRental.Interfaces.Module3.P2_1;

namespace ProRental.Domain.Module3.P2_1.Controls;

public sealed class BatchValidator : IBatchValidator
{
    private readonly AppDbContext _context;
    private readonly IDeliveryBatchMapper _deliveryBatchMapper;

    public BatchValidator(AppDbContext context, IDeliveryBatchMapper deliveryBatchMapper)
    {
        _context = context;
        _deliveryBatchMapper = deliveryBatchMapper;
    }

    public bool validateOrderExists(string orderId)
    {
        if (!int.TryParse(orderId, out var parsedOrderId))
        {
            return false;
        }

        return _context.Orders.Any(entity => EF.Property<int>(entity, "Orderid") == parsedOrderId);
    }

    public bool validateBatchExists(int batchId)
    {
        return _deliveryBatchMapper.findByIdentifier(batchId.ToString()) is not null;
    }
}
