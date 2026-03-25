using Microsoft.EntityFrameworkCore;
using ProRental.Data.Module3.P2_1.Interfaces;
using ProRental.Data.UnitOfWork;
using ProRental.Domain.Entities;

namespace ProRental.Data.Module3.P2_1;

public sealed class BatchOrderMapper : IBatchOrderMapper
{
    private readonly AppDbContext _context;

    public BatchOrderMapper(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> AddOrderToBatch(int batchId, int orderId, CancellationToken cancellationToken = default)
    {
        var existing = await _context.BatchOrders
            .AsNoTracking()
            .AnyAsync(item =>
                EF.Property<int>(item, "BatchId") == batchId &&
                EF.Property<int>(item, "OrderId") == orderId,
                cancellationToken);

        if (existing)
        {
            return false;
        }

        var batchOrder = new BatchOrder();
        batchOrder.Assign(batchId, orderId);
        await _context.BatchOrders.AddAsync(batchOrder, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> RemoveOrderFromBatch(int batchId, int orderId, CancellationToken cancellationToken = default)
    {
        var existing = await _context.BatchOrders
            .FirstOrDefaultAsync(item =>
                EF.Property<int>(item, "BatchId") == batchId &&
                EF.Property<int>(item, "OrderId") == orderId,
                cancellationToken);

        if (existing is null)
        {
            return false;
        }

        _context.BatchOrders.Remove(existing);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<int>> GetOrderIdsByBatch(int batchId, CancellationToken cancellationToken = default)
    {
        return await _context.BatchOrders
            .AsNoTracking()
            .Where(item => EF.Property<int>(item, "BatchId") == batchId)
            .Select(item => EF.Property<int>(item, "OrderId"))
            .ToListAsync(cancellationToken);
    }

    public async Task<int?> GetBatchIdByOrder(int orderId, CancellationToken cancellationToken = default)
    {
        var batchId = await _context.BatchOrders
            .AsNoTracking()
            .Where(item => EF.Property<int>(item, "OrderId") == orderId)
            .Select(item => (int?)EF.Property<int>(item, "BatchId"))
            .FirstOrDefaultAsync(cancellationToken);

        return batchId;
    }

    public Task<bool> OrderAlreadyAssigned(int orderId, CancellationToken cancellationToken = default)
    {
        return _context.BatchOrders
            .AsNoTracking()
            .AnyAsync(item => EF.Property<int>(item, "OrderId") == orderId, cancellationToken);
    }

    public Task<int> CountOrdersInBatch(int batchId, CancellationToken cancellationToken = default)
    {
        return _context.BatchOrders
            .AsNoTracking()
            .CountAsync(item => EF.Property<int>(item, "BatchId") == batchId, cancellationToken);
    }
}
