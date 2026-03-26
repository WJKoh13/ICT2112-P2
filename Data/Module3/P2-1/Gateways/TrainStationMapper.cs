using Microsoft.EntityFrameworkCore;
using ProRental.Data.UnitOfWork;
using ProRental.Domain.Entities;
using ProRental.Domain.Enums;

namespace ProRental.Data.Gateways;

/// <summary>
/// Handles TrainStation-specific persistence operations.
/// Inherits template methods from AbstractTransportationHubMapper.
/// </summary>
public class TrainStationMapper : AbstractTransportationHubMapper
{
    public TrainStationMapper(AppDbContext context) : base(context) { }

    public override TransportationHub? FindById(int hubId)
    {
        return _context.TransportationHubs
            .OfType<TrainStation>()
            .FirstOrDefault(h => EF.Property<int>(h, "HubId") == hubId);
    }

    public TrainStation? FindByTrainstationCode(string trainstationCode)
    {
        return _context.TransportationHubs
            .OfType<TrainStation>()
            .FirstOrDefault(t => EF.Property<string>(t, "TrainstationCode") == trainstationCode);
    }

    public List<TrainStation> FindByTrainstationName(string trainstationName)
    {
        return _context.TransportationHubs
            .OfType<TrainStation>()
            .Where(t => EF.Property<string>(t, "TrainstationName") == trainstationName)
            .ToList();
    }

    public override List<TransportationHub> FindByType(HubType hubType)
    {
        return _context.TransportationHubs
            .OfType<TrainStation>()
            .Where(h => EF.Property<HubType?>(h, "HubType") == hubType)
            .Cast<TransportationHub>()
            .ToList();
    }

    public override List<TransportationHub> FindAll()
    {
        return _context.TransportationHubs
            .OfType<TrainStation>()
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
        if (hub is not TrainStation trainStation) return;
        _context.TransportationHubs.Add(trainStation);
        _context.SaveChanges();
    }

    protected override void UpdateSubtypeRow(TransportationHub hub)
    {
        if (hub is not TrainStation trainStation) return;
        _context.TransportationHubs.Update(trainStation);
        _context.SaveChanges();
    }

    protected override void DeleteSubtypeRow(int hubId)
    {
        var trainStation = _context.TransportationHubs
            .OfType<TrainStation>()
            .FirstOrDefault(t => EF.Property<int>(t, "HubId") == hubId);
        if (trainStation != null)
        {
            _context.TransportationHubs.Remove(trainStation);
            _context.SaveChanges();
        }
    }

    protected override TransportationHub? LoadSubtypeRow(int hubId)
    {
        return _context.TransportationHubs
            .OfType<TrainStation>()
            .FirstOrDefault(h => EF.Property<int>(h, "HubId") == hubId);
    }
}
