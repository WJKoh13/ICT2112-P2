using Microsoft.EntityFrameworkCore;
using ProRental.Data.Module3.P2_1.Interfaces;
using ProRental.Data.UnitOfWork;
using ProRental.Domain.Entities;
using ProRental.Domain.Enums;

namespace ProRental.Data.Module3.P2_1.Gateways;

public sealed class DeliveryBatchMapper : IDeliveryBatchMapper
{
    private readonly AppDbContext _context;

    public DeliveryBatchMapper(AppDbContext context)
    {
        _context = context;
    }

    public DeliveryBatch createNewBatch(int destHubID, string destinationAddress)
    {
        var batch = new DeliveryBatch();
        batch.InitializeNewBatch(destHubID, destinationAddress);
        return batch;
    }

    public DeliveryBatch? findByIdentifier(string batchIdentifier)
    {
        if (!int.TryParse(batchIdentifier, out var parsedBatchId))
        {
            return null;
        }

        return _context.DeliveryBatches
            .Include(entity => entity.BatchOrders)
            .FirstOrDefault(entity => EF.Property<int>(entity, "DeliveryBatchId") == parsedBatchId);
    }

    public List<DeliveryBatch> findAll()
    {
        return _context.DeliveryBatches
            .Include(entity => entity.BatchOrders)
            .OrderBy(entity => EF.Property<int>(entity, "DeliveryBatchId"))
            .ToList();
    }

    public List<DeliveryBatch> getAllPendingBatches()
    {
        return _context.DeliveryBatches
            .Include(entity => entity.BatchOrders)
            .Where(entity => EF.Property<BatchStatus?>(entity, "DeliveryBatchStatus") == BatchStatus.PENDING)
            .OrderBy(entity => EF.Property<int>(entity, "DeliveryBatchId"))
            .ToList();
    }

    public List<DeliveryBatch> getBatchByStatus(string warehouse, BatchStatus status)
    {
        var query = _context.DeliveryBatches
            .Include(entity => entity.BatchOrders)
            .Where(entity => EF.Property<BatchStatus?>(entity, "DeliveryBatchStatus") == status);

        if (int.TryParse(warehouse, out var parsedHubId))
        {
            query = query.Where(entity => EF.Property<int?>(entity, "HubId") == parsedHubId);
        }
        else
        {
            query = query.Where(entity => EF.Property<string>(entity, "DestinationAddress") == warehouse);
        }

        return query.OrderBy(entity => EF.Property<int>(entity, "DeliveryBatchId")).ToList();
    }

    public bool insert(DeliveryBatch batch)
    {
        _context.DeliveryBatches.Add(batch);
        return _context.SaveChanges() > 0;
    }

    public bool update(DeliveryBatch batch)
    {
        _context.DeliveryBatches.Update(batch);
        return _context.SaveChanges() > 0;
    }

    public bool delete(string batchIdentifier)
    {
        var found = findByIdentifier(batchIdentifier);
        if (found is null)
        {
            return false;
        }

        _context.DeliveryBatches.Remove(found);
        return _context.SaveChanges() > 0;
    }
}
