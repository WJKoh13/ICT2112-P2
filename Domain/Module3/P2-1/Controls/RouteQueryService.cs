using ProRental.Domain.Entities;
using ProRental.Domain.Enums;
using ProRental.Interfaces.Module3.P2_1;

namespace ProRental.Domain.Module3.P2_1.Controls;

public sealed class RouteQueryService : IRouteQueryService
{
    public RouteQueryService()
    {
    }

    public RouteLeg? retrieveFirstMileLeg(int routeId)
    {
        return BuildFallbackFirstMileLeg();
    }

    private static RouteLeg BuildFallbackFirstMileLeg()
    {
        var fallback = new RouteLeg();
        fallback.ConfigureLeg(
            sequence: 1,
            startPoint: "1",
            endPoint: "1",
            distanceKm: 10d,
            transportMode: TransportMode.TRUCK,
            isFirstMile: true,
            isLastMile: false);
        return fallback;
    }
}
