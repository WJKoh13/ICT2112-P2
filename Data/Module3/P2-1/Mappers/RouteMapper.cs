using Microsoft.EntityFrameworkCore;
using ProRental.Data.Module3.P2_1.Interfaces;
using ProRental.Data.UnitOfWork;
using ProRental.Domain.Entities;

namespace ProRental.Data.Module3.P2_1.Mappers;

public sealed class RouteMapper : IRouteMapper
{
    private readonly AppDbContext _context;

    public RouteMapper(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(DeliveryRoute route, CancellationToken cancellationToken = default)
    {
        await _context.DeliveryRoutes.AddAsync(route, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }

    public RouteLeg? RetrieveMainTransportLeg(int routeId)
    {
        return _context.RouteLegs
            .AsNoTracking()
            .FirstOrDefault(routeLeg =>
                EF.Property<int>(routeLeg, "RouteId") == routeId &&
                routeLeg.ReadIsMainTransport() == true);
    }
}
