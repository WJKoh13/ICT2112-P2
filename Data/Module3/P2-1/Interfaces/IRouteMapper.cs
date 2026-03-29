using ProRental.Domain.Entities;

namespace ProRental.Data.Module3.P2_1.Interfaces;

public interface IRouteMapper
{
    Task AddAsync(DeliveryRoute route, CancellationToken cancellationToken = default);
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
    RouteLeg? RetrieveMainTransportLeg(int routeId);
}
