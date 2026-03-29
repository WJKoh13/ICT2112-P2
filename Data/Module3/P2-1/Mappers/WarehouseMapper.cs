using Microsoft.EntityFrameworkCore;
using ProRental.Data.UnitOfWork;
using ProRental.Domain.Entities;
using ProRental.Domain.Enums;

namespace ProRental.Data.Mappers;

/// <summary>
/// Handles Warehouse-specific persistence operations.
/// Inherits template methods from AbstractTransportationHubMapper.
/// </summary>
public class WarehouseMapper : AbstractTransportationHubMapper
{
    public WarehouseMapper(AppDbContext context) : base(context) { }

    public override TransportationHub? FindById(int hubId)
    {
        return _context.TransportationHubs
            .OfType<Warehouse>()
            .FirstOrDefault(h => EF.Property<int>(h, "HubId") == hubId);
    }

    public Warehouse? FindByWarehouseCode(string warehouseCode)
    {
        return _context.TransportationHubs
            .OfType<Warehouse>()
            .FirstOrDefault(w => EF.Property<string>(w, "WarehouseCode") == warehouseCode);
    }

    public override List<TransportationHub> FindByType(HubType hubType)
    {
        return _context.TransportationHubs
            .OfType<Warehouse>()
            .Where(h => EF.Property<HubType?>(h, "HubType") == hubType)
            .Cast<TransportationHub>()
            .ToList();
    }

    public override List<TransportationHub> FindAll()
    {
        return _context.TransportationHubs
            .OfType<Warehouse>()
            .Cast<TransportationHub>()
            .ToList();
    }

    public override void Insert(TransportationHub hub)
    {
        int hubId = InsertHubRow(hub);
        InsertSubtypeRow(hub, hubId);
    }

    public override void Update(TransportationHub hub)
    {
        UpdateHubRow(hub);
        UpdateSubtypeRow(hub);
    }

    public override void Delete(int hubId)
    {
        DeleteSubtypeRow(hubId);
        DeleteHubRow(hubId);
    }

    protected override void InsertSubtypeRow(TransportationHub hub, int hubId)
    {
        if (hub is not Warehouse warehouse) return;
        _context.TransportationHubs.Add(warehouse);
        _context.SaveChanges();
    }

    protected override void UpdateSubtypeRow(TransportationHub hub)
    {
        if (hub is not Warehouse warehouse) return;
        _context.TransportationHubs.Update(warehouse);
        _context.SaveChanges();
    }

    protected override void DeleteSubtypeRow(int hubId)
    {
        var warehouse = _context.TransportationHubs
            .OfType<Warehouse>()
            .FirstOrDefault(w => EF.Property<int>(w, "HubId") == hubId);
        if (warehouse != null)
        {
            _context.TransportationHubs.Remove(warehouse);
            _context.SaveChanges();
        }
    }

    protected override TransportationHub? LoadSubtypeRow(int hubId)
    {
        return _context.TransportationHubs
            .OfType<Warehouse>()
            .FirstOrDefault(h => EF.Property<int>(h, "HubId") == hubId);
    }
}
