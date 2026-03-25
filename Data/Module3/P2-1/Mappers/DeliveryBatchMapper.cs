using Microsoft.EntityFrameworkCore;
using ProRental.Data.Module3.P2_1.Interfaces;
using ProRental.Data.UnitOfWork;
using ProRental.Domain.Entities;
using ProRental.Domain.Enums;

namespace ProRental.Data.Module3.P2_1;

public sealed class DeliveryBatchMapper : IDeliveryBatchMapper
{
    private readonly AppDbContext _context;

    public DeliveryBatchMapper(AppDbContext context)
    {
        _context = context;
    }

    public DeliveryBatch CreateNewBatch(int destinationHubId, string destinationAddress)
    {
        var batch = new DeliveryBatch();
        batch.InitializeBatch(destinationHubId, destinationAddress);
        return batch;
    }

    public Task<DeliveryBatch?> FindByIdentifier(int batchIdentifier, CancellationToken cancellationToken = default)
    {
        return _context.DeliveryBatches
            .Include(item => item.BatchOrders)
            .AsNoTracking()
            .FirstOrDefaultAsync(item => EF.Property<int>(item, "DeliveryBatchId") == batchIdentifier, cancellationToken);
    }

    public async Task<IReadOnlyList<DeliveryBatch>> FindAll(CancellationToken cancellationToken = default)
    {
        return await _context.DeliveryBatches
            .Include(item => item.BatchOrders)
            .AsNoTracking()
            .OrderBy(item => EF.Property<int>(item, "DeliveryBatchId"))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DeliveryBatch>> GetAllPendingBatches(CancellationToken cancellationToken = default)
    {
        return await _context.DeliveryBatches
            .Include(item => item.BatchOrders)
            .AsNoTracking()
            .Where(item => EF.Property<BatchStatus?>(item, "DeliveryBatchStatus") == BatchStatus.PENDING)
            .OrderBy(item => EF.Property<int>(item, "DeliveryBatchId"))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DeliveryBatch>> GetBatchByStatus(
        int warehouseHubId,
        BatchStatus status,
        CancellationToken cancellationToken = default)
    {
        return await _context.DeliveryBatches
            .Include(item => item.BatchOrders)
            .AsNoTracking()
            .Where(item =>
                EF.Property<int?>(item, "HubId") == warehouseHubId &&
                EF.Property<BatchStatus?>(item, "DeliveryBatchStatus") == status)
            .OrderBy(item => EF.Property<int>(item, "DeliveryBatchId"))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> Insert(DeliveryBatch batch, CancellationToken cancellationToken = default)
    {
        await _context.DeliveryBatches.AddAsync(batch, cancellationToken);
        return await _context.SaveChangesAsync(cancellationToken) > 0;
    }

    public async Task<bool> Update(DeliveryBatch batch, CancellationToken cancellationToken = default)
    {
        _context.DeliveryBatches.Update(batch);
        return await _context.SaveChangesAsync(cancellationToken) > 0;
    }

    public async Task<bool> Delete(int batchIdentifier, CancellationToken cancellationToken = default)
    {
        var existing = await _context.DeliveryBatches
            .FirstOrDefaultAsync(item => EF.Property<int>(item, "DeliveryBatchId") == batchIdentifier, cancellationToken);

        if (existing is null)
        {
            return false;
        }

        _context.DeliveryBatches.Remove(existing);
        return await _context.SaveChangesAsync(cancellationToken) > 0;
    }
}
