using ProRental.Data.Interfaces;
using ProRental.Data.Module3.P2_1.Interfaces;
using ProRental.Domain.Entities;
using ProRental.Domain.Enums;
using ProRental.Interfaces.Module3.P2_1;
using ProRental.Models.Module3.P2_1;

namespace ProRental.Domain.Module3.P2_1.Controls;

/// <summary>
/// Main routing orchestrator for Module 3 / P2-1. It creates a structured
/// multi-modal route for Feature 1 and exposes main-transport route lookup for Feature 6.
/// </summary>
public sealed class RouteManager : IRoutingService, IRouteQueryService
{
    private readonly IRouteMapper _routeMapper;
    private readonly ITransportationHubMapper _transportationHubMapper;
    private readonly IRouteLegBuilder _routeLegBuilder;
    private readonly IRouteDistanceCalculator _routeDistanceCalculator;

    public RouteManager(
        IRouteMapper routeMapper,
        ITransportationHubMapper transportationHubMapper,
        IRouteLegBuilder routeLegBuilder,
        IRouteDistanceCalculator routeDistanceCalculator)
    {
        _routeMapper = routeMapper;
        _transportationHubMapper = transportationHubMapper;
        _routeLegBuilder = routeLegBuilder;
        _routeDistanceCalculator = routeDistanceCalculator;
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

        var routeModeProfile = RouteModeInputAdapter.ResolveProfile(modes);
        return await CreateRouteAsync(origin, destination, routeModeProfile);
    }

    private async Task<DeliveryRoute> CreateRouteAsync(string origin, string destination, RouteModeProfile routeModeProfile)
    {
        var routeContext = ResolveRouteContext(origin, destination, routeModeProfile);

        var route = new DeliveryRoute();
        route.InitializeRoute(routeContext.WarehouseAddress, routeContext.DestinationAddress);
        route.MarkAsValid();

        var routeLegs = await BuildRouteLegsAsync(routeContext);
        foreach (var routeLeg in routeLegs)
        {
            route.addLeg(routeLeg);
        }

        route.UpdateTotalDistanceKm((double)Math.Round(
            (decimal)routeLegs.Sum(routeLeg => routeLeg.ReadDistanceKm() ?? 0d),
            2,
            MidpointRounding.AwayFromZero));

        await _routeMapper.AddAsync(route);
        await _routeMapper.SaveChangesAsync();

        return route;
    }

    public RouteLeg retrieveMainTransportLeg(int routeId)
    {
        if (routeId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(routeId));
        }

        return _routeMapper.RetrieveMainTransportLeg(routeId)
            ?? throw new InvalidOperationException($"Main transport route leg for route '{routeId}' was not found.");
    }

    private async Task<List<RouteLeg>> BuildRouteLegsAsync(RouteContext routeContext)
    {
        var originHub = routeContext.OriginHub;
        var destinationHub = routeContext.DestinationHub;

        return routeContext.UseThreeLegRoute switch
        {
            true =>
            [
                await _routeLegBuilder.BuildFirstMileLegAsync(
                    routeContext.WarehouseHub,
                    originHub ?? throw new RouteResolutionException("Origin hub is required for multi-leg route generation."),
                    routeContext.FirstMileMode),
                await _routeLegBuilder.BuildMainTransportLegAsync(
                    originHub ?? throw new RouteResolutionException("Origin hub is required for multi-leg route generation."),
                    destinationHub ?? throw new RouteResolutionException("Destination hub is required for multi-leg route generation."),
                    routeContext.MainTransportMode),
                await _routeLegBuilder.BuildLastMileLegAsync(
                    destinationHub ?? throw new RouteResolutionException("Destination hub is required for multi-leg route generation."),
                    routeContext.DestinationAddress,
                    routeContext.LastMileMode)
            ],
            false =>
            [
                await BuildDirectMainLegAsync(routeContext)
            ],
        };
    }

    private async Task<RouteLeg> BuildDirectMainLegAsync(RouteContext routeContext)
    {
        return await BuildDirectLegAsync(
            routeContext.WarehouseAddress,
            routeContext.DestinationAddress,
            routeContext.MainTransportMode);
    }

    private RouteContext ResolveRouteContext(string origin, string destination, RouteModeProfile routeModeProfile)
    {
        var warehouseHub = ResolveWarehouseHub();
        var customerAddress = EnsureAddress(destination, "Customer destination address");
        var warehouseCountryCode = RouteCountryCodeResolver.ResolveWarehouseCountryCode(warehouseHub);
        var destinationCountryCode = RouteCountryCodeResolver.ResolveAddressCountryCode(customerAddress, "Customer destination address");

        return routeModeProfile switch
        {
            { UseThreeLegRoute: true, MainTransportMode: TransportMode.PLANE } => ResolveHubRouteContext(
                warehouseHub,
                customerAddress,
                warehouseCountryCode,
                destinationCountryCode,
                routeModeProfile,
                HubType.AIRPORT,
                "airport"),
            { UseThreeLegRoute: true, MainTransportMode: TransportMode.SHIP } => ResolveHubRouteContext(
                warehouseHub,
                customerAddress,
                warehouseCountryCode,
                destinationCountryCode,
                routeModeProfile,
                HubType.SHIPPING_PORT,
                "shipping port"),
            { UseThreeLegRoute: false, MainTransportMode: TransportMode.TRAIN or TransportMode.TRUCK } => new RouteContext(
                warehouseHub,
                customerAddress,
                routeModeProfile.FirstMileMode,
                routeModeProfile.MainTransportMode,
                routeModeProfile.LastMileMode,
                UseThreeLegRoute: false,
                OriginHub: null,
                DestinationHub: null),
            _ => throw new RouteResolutionException($"Transport mode profile '{routeModeProfile.MainTransportMode}' is not supported for route generation.")
        };
    }

    private RouteContext ResolveHubRouteContext(
        TransportationHub warehouseHub,
        string customerAddress,
        string warehouseCountryCode,
        string destinationCountryCode,
        RouteModeProfile routeModeProfile,
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
            routeModeProfile.MainTransportMode,
            "warehouse");

        var destinationHub = ResolveCountryMatchedHub(
            hubsByAddress,
            destinationCountryCode,
            hubLabel,
            routeModeProfile.MainTransportMode,
            "customer destination",
            excludedAddress: originHub.GetAddress());

        if (string.Equals(originHub.GetAddress(), destinationHub.GetAddress(), StringComparison.OrdinalIgnoreCase))
        {
            throw new RouteResolutionException(
                $"Distinct {hubLabel} addresses are required for {routeModeProfile.MainTransportMode} route generation between '{warehouseCountryCode}' and '{destinationCountryCode}'.");
        }

        return new RouteContext(
            warehouseHub,
            customerAddress,
            routeModeProfile.FirstMileMode,
            routeModeProfile.MainTransportMode,
            routeModeProfile.LastMileMode,
            UseThreeLegRoute: true,
            originHub,
            destinationHub);
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

    private async Task<RouteLeg> BuildDirectLegAsync(string originAddress, string destinationAddress, TransportMode transportMode)
    {
        var originPoint = new RouteDistancePoint(EnsureAddress(originAddress, "Warehouse address"));
        var destinationPoint = new RouteDistancePoint(EnsureAddress(destinationAddress, "Customer destination address"));
        var distanceKm = await _routeDistanceCalculator.CalculateLegDistanceKmAsync(transportMode, originPoint, destinationPoint);

        var routeLeg = new RouteLeg();
        routeLeg.ConfigureLeg(
            sequence: 1,
            originPoint.Address,
            destinationPoint.Address,
            distanceKm,
            transportMode,
            isFirstMile: false,
            isMainTransport: true,
            isLastMile: false);
        return routeLeg;
    }

    private sealed record RouteContext(
        TransportationHub WarehouseHub,
        string DestinationAddress,
        TransportMode FirstMileMode,
        TransportMode MainTransportMode,
        TransportMode LastMileMode,
        bool UseThreeLegRoute,
        TransportationHub? OriginHub,
        TransportationHub? DestinationHub)
    {
        public string WarehouseAddress => EnsureAddress(WarehouseHub.GetAddress(), "Warehouse address");
    }
}
