using ProRental.Domain.Entities;

namespace ProRental.Interfaces.Module3.P2_1;

public interface IRouteQueryService
{
    RouteLeg? retrieveFirstMileLeg(int routeId);
}
