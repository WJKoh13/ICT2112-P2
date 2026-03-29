using Microsoft.EntityFrameworkCore;
using ProRental.Data.UnitOfWork;
using ProRental.Domain.Entities;
using ProRental.Domain.Enums;

namespace ProRental.Data.Mappers;

/// <summary>
/// Handles ShippingPort-specific persistence operations.
/// Inherits template methods from AbstractTransportationHubMapper.
/// </summary>
public class ShippingPortMapper : AbstractTransportationHubMapper
{
    public ShippingPortMapper(AppDbContext context) : base(context) { }

    public override TransportationHub? FindById(int hubId)
    {
        return _context.TransportationHubs
            .OfType<ShippingPort>()
            .FirstOrDefault(h => EF.Property<int>(h, "HubId") == hubId);
    }

    public ShippingPort? FindByPortCode(string portCode)
    {
        return _context.TransportationHubs
            .OfType<ShippingPort>()
            .FirstOrDefault(s => EF.Property<string>(s, "PortCode") == portCode);
    }

    public List<ShippingPort> FindByPortName(string portName)
    {
        return _context.TransportationHubs
            .OfType<ShippingPort>()
            .Where(s => EF.Property<string>(s, "PortName") == portName)
            .ToList();
    }

    public override List<TransportationHub> FindByType(HubType hubType)
    {
        return _context.TransportationHubs
            .OfType<ShippingPort>()
            .Where(h => EF.Property<HubType?>(h, "HubType") == hubType)
            .Cast<TransportationHub>()
            .ToList();
    }

    public override List<TransportationHub> FindAll()
    {
        return _context.TransportationHubs
            .OfType<ShippingPort>()
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
        if (hub is not ShippingPort port) return;
        _context.TransportationHubs.Add(port);
        _context.SaveChanges();
    }

    protected override void UpdateSubtypeRow(TransportationHub hub)
    {
        if (hub is not ShippingPort port) return;
        _context.TransportationHubs.Update(port);
        _context.SaveChanges();
    }

    protected override void DeleteSubtypeRow(int hubId)
    {
        var port = _context.TransportationHubs
            .OfType<ShippingPort>()
            .FirstOrDefault(s => EF.Property<int>(s, "HubId") == hubId);
        if (port != null)
        {
            _context.TransportationHubs.Remove(port);
            _context.SaveChanges();
        }
    }

    protected override TransportationHub? LoadSubtypeRow(int hubId)
    {
        return _context.TransportationHubs
            .OfType<ShippingPort>()
            .FirstOrDefault(h => EF.Property<int>(h, "HubId") == hubId);
    }
}
