using Microsoft.EntityFrameworkCore;
using ProRental.Data.Interfaces;
using ProRental.Data.UnitOfWork;
using ProRental.Domain.Entities;
using ProRental.Domain.Enums;
using ProRental.Interfaces.Module3.P2_1;

namespace ProRental.Domain.Controls;

/// <summary>
/// Main routing orchestrator for Module 3 / P2-1. It creates a structured
/// multi-modal route for Feature 1 and exposes first-mile route lookup for Feature 6.
/// </summary>
public sealed class RouteManager : IRoutingService, IRouteQueryService
{
    private readonly AppDbContext _context;
    private readonly ITransportationHubMapper _transportationHubMapper;
    private readonly IRouteLegBuilder _routeLegBuilder;

    public RouteManager(
        AppDbContext context,
        ITransportationHubMapper transportationHubMapper,
        IRouteLegBuilder routeLegBuilder)
    {
        _context = context;
        _transportationHubMapper = transportationHubMapper;
        _routeLegBuilder = routeLegBuilder;
    }

    public async Task<DeliveryRoute> CreateMultiModalRouteAsync(string origin, string destination, List<TransportMode> modes)
    {
        if (string.IsNullOrWhiteSpace(origin))
        {
            throw new ArgumentException("Origin is required.", nameof(origin));
        }

        if (string.IsNullOrWhiteSpace(destination))
        {
            throw new ArgumentException("Destination is required.", nameof(destination));
        }

        if (modes is null || modes.Count == 0)
        {
            throw new ArgumentException("At least one transport mode is required.", nameof(modes));
        }

        var orderedModes = modes
            .Distinct()
            .ToList();

        for (var index = 0; index < orderedModes.Count; index++)
        {
            var mode = orderedModes[index];

            try
            {
                return await CreateRouteForModeAsync(origin, destination, mode);
            }
            catch (RouteResolutionException exception) when (index < orderedModes.Count - 1 && ShouldTryNextMode(mode, orderedModes[index + 1], exception))
            {
                continue;
            }
        }

        throw new InvalidOperationException("Route resolution exhausted all transport-mode fallbacks without producing a route.");
    }

    private async Task<DeliveryRoute> CreateRouteForModeAsync(string origin, string destination, TransportMode mode)
    {
        var routeContext = ResolveRouteContext(origin, destination, mode);

        var route = new DeliveryRoute();
        route.SetOriginAddress(routeContext.WarehousePoint.Address);
        route.SetDestinationAddress(routeContext.DestinationPoint.Address);
        route.SetIsValid(true);

        var routeLegs = await BuildRouteLegsAsync(routeContext);
        foreach (var routeLeg in routeLegs)
        {
            route.RouteLegs.Add(routeLeg);
        }

        route.SetTotalDistanceKm((double)Math.Round(
            (decimal)routeLegs.Sum(routeLeg => routeLeg.GetDistanceKm() ?? 0d),
            2,
            MidpointRounding.AwayFromZero));

        _context.DeliveryRoutes.Add(route);
        await _context.SaveChangesAsync();

        return route;
    }

    public RouteLeg retrieveFirstMileLeg(int routeId)
    {
        if (routeId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(routeId));
        }

        return _context.RouteLegs
            .AsNoTracking()
            .FirstOrDefault(routeLeg =>
                EF.Property<int>(routeLeg, "RouteId") == routeId &&
                routeLeg.GetIsFirstMile() == true)
            ?? throw new InvalidOperationException($"First-mile route leg for route '{routeId}' was not found.");
    }

    private async Task<List<RouteLeg>> BuildRouteLegsAsync(RouteContext routeContext)
    {
        var originHubPoint = routeContext.OriginHubPoint;
        var destinationHubPoint = routeContext.DestinationHubPoint;

        return routeContext.MainTransportMode switch
        {
            TransportMode.PLANE or TransportMode.SHIP =>
            [
                await _routeLegBuilder.BuildFirstMileLegAsync(routeContext.WarehousePoint, originHubPoint ?? throw new RouteResolutionException("Origin hub address is required for multi-leg route generation.")),
                await _routeLegBuilder.BuildMainTransportLegAsync(2, originHubPoint ?? throw new RouteResolutionException("Origin hub address is required for multi-leg route generation."), destinationHubPoint ?? throw new RouteResolutionException("Destination hub address is required for multi-leg route generation."), routeContext.MainTransportMode),
                await _routeLegBuilder.BuildLastMileLegAsync(3, destinationHubPoint ?? throw new RouteResolutionException("Destination hub address is required for multi-leg route generation."), routeContext.DestinationPoint)
            ],
            TransportMode.TRAIN or TransportMode.TRUCK =>
            [
                await _routeLegBuilder.BuildMainTransportLegAsync(1, routeContext.WarehousePoint, routeContext.DestinationPoint, routeContext.MainTransportMode)
            ],
            _ => throw new RouteResolutionException($"Transport mode '{routeContext.MainTransportMode}' is not supported for route generation.")
        };
    }

    private RouteContext ResolveRouteContext(string origin, string destination, TransportMode mainTransportMode)
    {
        var warehouseHub = ResolveWarehouseHub();
        var customerAddress = EnsureAddress(destination, "Customer destination address");
        var warehousePoint = ToRouteDistancePoint(warehouseHub);
        var customerPoint = new RouteDistancePoint(customerAddress);
        var warehouseCountryCode = RouteCountryCodeResolver.ResolveWarehouseCountryCode(warehouseHub);
        var destinationCountryCode = RouteCountryCodeResolver.ResolveAddressCountryCode(customerAddress, "Customer destination address");

        return mainTransportMode switch
        {
            TransportMode.PLANE => ResolveHubRouteContext(warehousePoint, customerPoint, warehouseCountryCode, destinationCountryCode, TransportMode.PLANE, HubType.AIRPORT, "airport"),
            TransportMode.SHIP => ResolveHubRouteContext(warehousePoint, customerPoint, warehouseCountryCode, destinationCountryCode, TransportMode.SHIP, HubType.SHIPPING_PORT, "shipping port"),
            TransportMode.TRAIN or TransportMode.TRUCK => new RouteContext(
                warehousePoint,
                customerPoint,
                mainTransportMode,
                OriginHubPoint: null,
                DestinationHubPoint: null),
            _ => throw new RouteResolutionException($"Transport mode '{mainTransportMode}' is not supported for route generation.")
        };
    }

    private static bool ShouldTryNextMode(TransportMode currentMode, TransportMode nextMode, RouteResolutionException exception)
    {
        return currentMode == TransportMode.TRAIN &&
               nextMode == TransportMode.SHIP &&
               exception.Message.Contains("did not return a route distance", StringComparison.Ordinal);
    }

    private RouteContext ResolveHubRouteContext(
        RouteDistancePoint warehousePoint,
        RouteDistancePoint customerPoint,
        string warehouseCountryCode,
        string destinationCountryCode,
        TransportMode mainTransportMode,
        HubType hubType,
        string hubLabel)
    {
        var hubsByAddress = _transportationHubMapper.FindByType(hubType)
            .Where(hub => !string.IsNullOrWhiteSpace(hub.GetAddress()))
            .GroupBy(hub => hub.GetAddress(), StringComparer.OrdinalIgnoreCase)
            .Select(group => group.OrderBy(hub => hub.GetHubId()).First())
            .ToList();

        var originHub = ResolveCountryMatchedHub(
            hubsByAddress,
            warehouseCountryCode,
            hubLabel,
            mainTransportMode,
            "warehouse");

        var destinationHub = ResolveCountryMatchedHub(
            hubsByAddress,
            destinationCountryCode,
            hubLabel,
            mainTransportMode,
            "customer destination",
            excludedAddress: originHub.GetAddress());

        if (string.Equals(originHub.GetAddress(), destinationHub.GetAddress(), StringComparison.OrdinalIgnoreCase))
        {
            throw new RouteResolutionException(
                $"Distinct {hubLabel} addresses are required for {mainTransportMode} route generation between '{warehouseCountryCode}' and '{destinationCountryCode}'.");
        }

        return new RouteContext(
            warehousePoint,
            customerPoint,
            mainTransportMode,
            ToRouteDistancePoint(originHub),
            ToRouteDistancePoint(destinationHub));
    }

    private TransportationHub ResolveWarehouseHub()
    {
        var warehouseHub = _transportationHubMapper.FindByType(HubType.WAREHOUSE)
            .FirstOrDefault(hub => !string.IsNullOrWhiteSpace(hub.GetAddress()));

        if (warehouseHub is null)
        {
            throw new RouteResolutionException("A warehouse address is required for shipping route generation.");
        }

        return warehouseHub;
    }

    private TransportationHub ResolveCountryMatchedHub(
        IReadOnlyList<TransportationHub> hubs,
        string countryCode,
        string hubLabel,
        TransportMode mainTransportMode,
        string routeSide,
        string? excludedAddress = null)
    {
        var matchedHub = hubs
            .Where(hub => string.Equals(RouteCountryCodeResolver.NormalizeCountryCode(hub.GetCountryCode()), countryCode, StringComparison.OrdinalIgnoreCase))
            .Where(hub => excludedAddress is null || !string.Equals(hub.GetAddress(), excludedAddress, StringComparison.OrdinalIgnoreCase))
            .OrderBy(hub => hub.GetHubId())
            .FirstOrDefault();

        if (matchedHub is not null)
        {
            return matchedHub;
        }

        if (excludedAddress is not null)
        {
            var sameCountryHub = hubs.FirstOrDefault(hub =>
                string.Equals(RouteCountryCodeResolver.NormalizeCountryCode(hub.GetCountryCode()), countryCode, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(hub.GetAddress(), excludedAddress, StringComparison.OrdinalIgnoreCase));

            if (sameCountryHub is not null)
            {
                throw new RouteResolutionException(
                    $"Distinct {hubLabel} addresses are required for {mainTransportMode} route generation when both legs resolve to country '{countryCode}'.");
            }
        }

        throw new RouteResolutionException(
            $"No {hubLabel} hub is configured for the {routeSide} country '{countryCode}' required by {mainTransportMode} route generation.");
    }

    private static string EnsureAddress(string address, string label)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            throw new RouteResolutionException($"{label} is required for shipping route generation.");
        }

        return address;
    }

    private static RouteDistancePoint ToRouteDistancePoint(TransportationHub hub)
    {
        return new RouteDistancePoint(
            EnsureAddress(hub.GetAddress(), "Hub address"),
            hub.GetLatitude(),
            hub.GetLongitude());
    }

    private sealed record RouteContext(
        RouteDistancePoint WarehousePoint,
        RouteDistancePoint DestinationPoint,
        TransportMode MainTransportMode,
        RouteDistancePoint? OriginHubPoint,
        RouteDistancePoint? DestinationHubPoint);
}
