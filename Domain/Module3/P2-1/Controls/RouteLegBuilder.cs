using ProRental.Domain.Entities;
using ProRental.Domain.Enums;
using ProRental.Interfaces.Module3.P2_1;

namespace ProRental.Domain.Controls;

/// <summary>
/// Builds route legs for the routing workflow while keeping leg construction
/// separate from route orchestration.
/// </summary>
public sealed class RouteLegBuilder : IRouteLegBuilder
{
    private readonly IRouteDistanceCalculator _routeDistanceCalculator;

    public RouteLegBuilder(IRouteDistanceCalculator routeDistanceCalculator)
    {
        _routeDistanceCalculator = routeDistanceCalculator;
    }

    public RouteLeg BuildFirstMileLeg(string startPoint, string endPoint)
    {
        return BuildLeg(1, startPoint, endPoint, TransportMode.TRUCK, isFirstMile: true, isMainTransport: false, isLastMile: false);
    }

    public RouteLeg BuildMainTransportLeg(int sequence, string startPoint, string endPoint, TransportMode transportMode)
    {
        return BuildLeg(sequence, startPoint, endPoint, transportMode, isFirstMile: false, isMainTransport: true, isLastMile: false);
    }

    public RouteLeg BuildLastMileLeg(int sequence, string startPoint, string endPoint)
    {
        return BuildLeg(sequence, startPoint, endPoint, TransportMode.TRUCK, isFirstMile: false, isMainTransport: false, isLastMile: true);
    }

    private RouteLeg BuildLeg(
        int sequence,
        string startPoint,
        string endPoint,
        TransportMode transportMode,
        bool isFirstMile,
        bool isMainTransport,
        bool isLastMile)
    {
        var distanceKm = _routeDistanceCalculator.CalculateDistanceKm(startPoint, endPoint);
        var routeLeg = new RouteLeg();
        routeLeg.ConfigureLeg(sequence, startPoint, endPoint, distanceKm, transportMode, isFirstMile, isMainTransport, isLastMile);
        return routeLeg;
    }
}
