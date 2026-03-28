using Microsoft.EntityFrameworkCore;
using ProRental.Data.Module3.P2_1.Interfaces;
using ProRental.Data.UnitOfWork;
using ProRental.Domain.Entities;

namespace ProRental.Data.Module3.P2_1.Gateways;

public sealed class BatchOrderMapper : IBatchOrderMapper
{
    private readonly AppDbContext _context;

    public BatchOrderMapper(AppDbContext context)
    {
        _context = context;
    }

    public bool addOrderToBatch(int batchId, string orderId)
    {
        if (!int.TryParse(orderId, out var parsedOrderId))
        {
            return false;
        }

        if (orderAlreadyAssigned(orderId))
        {
            return false;
        }

        var link = new BatchOrder();
        var entry = _context.Entry(link);
        entry.Property("BatchId").CurrentValue = batchId;
        entry.Property("OrderId").CurrentValue = parsedOrderId;
        entry.Property("AddedTimestamp").CurrentValue = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);

        _context.BatchOrders.Add(link);
        return _context.SaveChanges() > 0;
    }

    public bool removeOrderFromBatch(string batchId, string orderId)
    {
        if (!int.TryParse(batchId, out var parsedBatchId) || !int.TryParse(orderId, out var parsedOrderId))
        {
            return false;
        }

        var link = _context.BatchOrders
            .FirstOrDefault(entity =>
                EF.Property<int>(entity, "BatchId") == parsedBatchId
                && EF.Property<int>(entity, "OrderId") == parsedOrderId);

        if (link is null)
        {
            return false;
        }

        _context.BatchOrders.Remove(link);
        return _context.SaveChanges() > 0;
    }

    // Clears all batch-order link records for admin/demo reset flows.
    // Utility function not exposed in domain layer as part of main requirements, but added to support easier testing and demoing.
    public bool clearAllOrderBatchLinks()
    {
        var links = _context.BatchOrders.ToList();
        if (links.Count == 0)
        {
            return true;
        }

        _context.BatchOrders.RemoveRange(links);
        return _context.SaveChanges() >= 0;
    }

    public List<string> getOrderIdsByBatch(int batchId)
    {
        return _context.BatchOrders
            .Where(entity => EF.Property<int>(entity, "BatchId") == batchId)
            .Select(entity => EF.Property<int>(entity, "OrderId").ToString())
            .ToList();
    }

    public string? getBatchIdByOrder(string orderId)
    {
        if (!int.TryParse(orderId, out var parsedOrderId))
        {
            return null;
        }

        var batchId = _context.BatchOrders
            .Where(entity => EF.Property<int>(entity, "OrderId") == parsedOrderId)
            .Select(entity => (int?)EF.Property<int>(entity, "BatchId"))
            .FirstOrDefault();

        return batchId?.ToString();
    }

    public bool orderAlreadyAssigned(string orderId)
    {
        if (!int.TryParse(orderId, out var parsedOrderId))
        {
            return false;
        }

        return _context.BatchOrders.Any(entity => EF.Property<int>(entity, "OrderId") == parsedOrderId);
    }

    public int countOrdersInBatch(int batchId)
    {
        return _context.BatchOrders.Count(entity => EF.Property<int>(entity, "BatchId") == batchId);
    }

    public double getOrderWeightKg(int orderId)
    {
        var orderWeightKg = (from orderItem in _context.Orderitems
                             join productDetail in _context.Productdetails
                                 on EF.Property<int>(orderItem, "Productid") equals EF.Property<int>(productDetail, "Productid")
                             where EF.Property<int>(orderItem, "Orderid") == orderId
                             select (double?)EF.Property<int>(orderItem, "Quantity") *
                                    (double?)(EF.Property<decimal?>(productDetail, "Weight") ?? 1m))
            .Sum() ?? 0d;

        return orderWeightKg > 0d ? orderWeightKg : 1d;
    }
}
