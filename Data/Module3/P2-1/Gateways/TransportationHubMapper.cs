using Microsoft.EntityFrameworkCore;
using ProRental.Data.UnitOfWork;
using ProRental.Domain.Entities;
using ProRental.Domain.Enums;

namespace ProRental.Data.Gateways;

/// <summary>
/// Concrete mapper inheriting from AbstractTransportationHubMapper.
/// Composes WarehouseMapper, AirportMapper, and ShippingPortMapper
/// to handle persistence for all TransportationHub subtypes.
/// </summary>
public class TransportationHubMapper : AbstractTransportationHubMapper
{
    // Composition: sub-mappers for each hub subtype
    private readonly WarehouseMapper _warehouseMapper;
    private readonly AirportMapper _airportMapper;
    private readonly ShippingPortMapper _shippingPortMapper;

    public TransportationHubMapper(AppDbContext context) : base(context)
    {
        _warehouseMapper = new WarehouseMapper(context);
        _airportMapper = new AirportMapper(context);
        _shippingPortMapper = new ShippingPortMapper(context);
    }

    public override TransportationHub? FindById(int hubId)
    {
        return _warehouseMapper.FindById(hubId)
            ?? _airportMapper.FindById(hubId)
            ?? _shippingPortMapper.FindById(hubId);
    }

    public override List<TransportationHub> FindByType(HubType hubType)
    {
        return hubType switch
        {
            HubType.WAREHOUSE => _warehouseMapper.FindByType(hubType),
            HubType.AIRPORT => _airportMapper.FindByType(hubType),
            HubType.SHIPPING_PORT => _shippingPortMapper.FindByType(hubType),
            HubType.TRAIN_STATION => [],
            _ => []
        };
    }

    public override List<TransportationHub> FindAll()
    {
        return _warehouseMapper.FindAll()
            .Concat(_airportMapper.FindAll())
            .Concat(_shippingPortMapper.FindAll())
            .OrderBy(hub => hub.GetHubId())
            .ToList();
    }

    public override void Insert(TransportationHub hub)
    {
        switch (hub)
        {
            case Warehouse:
                _warehouseMapper.Insert(hub);
                return;
            case Airport:
                _airportMapper.Insert(hub);
                return;
            case ShippingPort:
                _shippingPortMapper.Insert(hub);
                return;
            case TrainStation:
                throw new InvalidOperationException("TrainStation persistence is not supported by the current EF mapping.");
            default:
                throw new InvalidOperationException("Unsupported transportation hub subtype for insert.");
        }
    }

    public override void Update(TransportationHub hub)
    {
        switch (hub)
        {
            case Warehouse:
                _warehouseMapper.Update(hub);
                return;
            case Airport:
                _airportMapper.Update(hub);
                return;
            case ShippingPort:
                _shippingPortMapper.Update(hub);
                return;
            case TrainStation:
                throw new InvalidOperationException("TrainStation persistence is not supported by the current EF mapping.");
            default:
                throw new InvalidOperationException("Unsupported transportation hub subtype for update.");
        }
    }

    public override void Delete(int hubId)
    {
        var warehouse = _warehouseMapper.FindById(hubId);
        if (warehouse is not null)
        {
            _warehouseMapper.Delete(hubId);
            return;
        }

        var airport = _airportMapper.FindById(hubId);
        if (airport is not null)
        {
            _airportMapper.Delete(hubId);
            return;
        }

        var shippingPort = _shippingPortMapper.FindById(hubId);
        if (shippingPort is not null)
        {
            _shippingPortMapper.Delete(hubId);
            return;
        }

        return;
    }

    protected override void InsertSubtypeRow(TransportationHub hub, int hubId)
    {
        // Delegated to composed sub-mappers as needed
    }

    protected override void UpdateSubtypeRow(TransportationHub hub)
    {
        // Delegated to composed sub-mappers as needed
    }

    protected override void DeleteSubtypeRow(int hubId)
    {
        // Delegated to composed sub-mappers as needed
    }

    protected override TransportationHub? LoadSubtypeRow(int hubId)
    {
        // Delegated to composed sub-mappers as needed
        return null;
    }
}
