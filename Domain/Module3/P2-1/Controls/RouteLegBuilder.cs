using ProRental.Domain.Entities;
using ProRental.Domain.Enums;
using ProRental.Interfaces.Module3.P2_1;

namespace ProRental.Domain.Module3.P2_1.Controls;

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

    public Task<RouteLeg> BuildFirstMileLegAsync(TransportationHub warehouse, TransportationHub originHub, TransportMode transportMode)
    {
        return BuildLegAsync(
            sequence: 1,
            ToRouteDistancePoint(warehouse),
            ToRouteDistancePoint(originHub),
            transportMode,
            isFirstMile: true,
            isMainTransport: false,
            isLastMile: false);
    }

    public Task<RouteLeg> BuildMainTransportLegAsync(TransportationHub originHub, TransportationHub destinationHub, TransportMode transportMode)
    {
        return BuildLegAsync(
            sequence: 2,
            ToRouteDistancePoint(originHub),
            ToRouteDistancePoint(destinationHub),
            transportMode,
            isFirstMile: false,
            isMainTransport: true,
            isLastMile: false);
    }

    public Task<RouteLeg> BuildLastMileLegAsync(TransportationHub destinationHub, string customerAddress, TransportMode transportMode)
    {
        return BuildLegAsync(
            sequence: 3,
            ToRouteDistancePoint(destinationHub),
            ToRouteDistancePoint(customerAddress),
            transportMode,
            isFirstMile: false,
            isMainTransport: false,
            isLastMile: true);
    }

    private async Task<RouteLeg> BuildLegAsync(
        int sequence,
        RouteDistancePoint startPoint,
        RouteDistancePoint endPoint,
        TransportMode transportMode,
        bool isFirstMile,
        bool isMainTransport,
        bool isLastMile)
    {
        var distanceKm = await _routeDistanceCalculator.CalculateLegDistanceKmAsync(transportMode, startPoint, endPoint);
        var routeLeg = new RouteLeg();
        routeLeg.ConfigureLeg(sequence, startPoint.Address, endPoint.Address, distanceKm, transportMode, isFirstMile, isMainTransport, isLastMile);
        return routeLeg;
    }

    private static RouteDistancePoint ToRouteDistancePoint(TransportationHub hub)
    {
        return new RouteDistancePoint(
            EnsureAddress(hub.GetAddress(), "Hub address"),
            hub.GetLatitude(),
            hub.GetLongitude());
    }

    private static RouteDistancePoint ToRouteDistancePoint(string address)
    {
        return new RouteDistancePoint(EnsureAddress(address, "Customer destination address"));
    }

    private static string EnsureAddress(string address, string label)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            throw new RouteResolutionException($"{label} is required for shipping route generation.");
        }

        return address;
    }
}
