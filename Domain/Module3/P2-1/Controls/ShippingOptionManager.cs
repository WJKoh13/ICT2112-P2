using ProRental.Data.UnitOfWork;
using ProRental.Domain.Entities;
using ProRental.Domain.Enums;
using ProRental.Data.Interfaces;
using ProRental.Interfaces.Module3.P2_1;
using ProRental.Models.Module3.P2_1;
using ProRental.Domain.Module3.P2_1.Controls;

namespace ProRental.Domain.Controls;

/// <summary>
/// Feature 1 application service. It first exposes lightweight preference cards and
/// only generates a real route, quote, and persisted shipping option after selection.
/// by: ernest
/// </summary>
public sealed class ShippingOptionManager : IShippingOptionService
{
    private const string DefaultOrigin = "ProRental Warehouse";

    private static readonly PreferenceType[] PreferenceOrder =
    [
        PreferenceType.FAST,
        PreferenceType.CHEAP,
        PreferenceType.GREEN
    ];

    private readonly IShippingOptionMapper _shippingOptionMapper;
    private readonly IOrderService _orderService;
    private readonly IRoutingService _routingService;
    private readonly ITransportCarbonService _transportCarbonService;
    private readonly ITransportationHubMapper? _transportationHubMapper;
    private readonly AppDbContext? _context;

    public ShippingOptionManager(
        IShippingOptionMapper shippingOptionMapper,
        IOrderService orderService,
        IRoutingService routingService,
        ITransportationHubMapper transportationHubMapper,
        ITransportCarbonService transportCarbonService,
        AppDbContext context)
    {
        _shippingOptionMapper = shippingOptionMapper;
        _orderService = orderService;
        _routingService = routingService;
        _transportationHubMapper = transportationHubMapper;
        _transportCarbonService = transportCarbonService;
        _context = context;
    }

    internal ShippingOptionManager(
        IShippingOptionMapper shippingOptionMapper,
        IOrderService orderService,
        IRoutingService routingService,
        ITransportationHubMapper? transportationHubMapper,
        ITransportCarbonService transportCarbonService)
    {
        _shippingOptionMapper = shippingOptionMapper;
        _orderService = orderService;
        _routingService = routingService;
        _transportationHubMapper = transportationHubMapper;
        _transportCarbonService = transportCarbonService;
        _context = null;
    }

    public async Task<IReadOnlyList<ShippingPreferenceCard>> GetPreferenceChoicesForOrderAsync(
        int orderId,
        CancellationToken cancellationToken = default)
    {
        var context = await _orderService.GetShippingContextAsync(orderId, cancellationToken)
            ?? throw new InvalidOperationException($"Order '{orderId}' was not found.");
        var isSameCountry = IsSameCountryRoute(context);

        return PreferenceOrder
            .Select(preferenceType =>
            {
                var profile = GetPreferenceProfile(preferenceType);

                return new ShippingPreferenceCard(
                    context.OrderId,
                    preferenceType,
                    profile.DisplayName,
                    profile.Description,
                    PreferenceTypeModes.GetAllowedModesLabel(preferenceType, isSameCountry));
            })
            .ToArray();
    }

    public async Task<ShippingSelectionResult> ConfirmPreferenceSelectionAsync(
        SelectShippingPreferenceRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.OrderId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(request.OrderId));
        }

        if (_context is null || _context.Database.CurrentTransaction is not null)
        {
            return await ConfirmPreferenceSelectionCoreAsync(request, cancellationToken);
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        var result = await ConfirmPreferenceSelectionCoreAsync(request, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return result;
    }

    private async Task<ShippingSelectionResult> ConfirmPreferenceSelectionCoreAsync(
        SelectShippingPreferenceRequest request,
        CancellationToken cancellationToken)
    {
        var context = await _orderService.GetShippingContextAsync(request.OrderId, cancellationToken)
            ?? throw new InvalidOperationException($"Order '{request.OrderId}' was not found.");

        var order = await _shippingOptionMapper.FindOrderWithCheckoutAsync(request.OrderId, cancellationToken)
            ?? throw new InvalidOperationException($"Order '{request.OrderId}' was not found.");

        var checkoutId = order.GetOrderContext().CheckoutId;
        if (checkoutId <= 0)
        {
            throw new InvalidOperationException($"Order '{request.OrderId}' does not have a checkout record.");
        }

        var profile = GetPreferenceProfile(request.PreferenceType);
        var routeModeProfile = PreferenceTypeModes.ResolveRouteProfile(request.PreferenceType, IsSameCountryRoute(context));
        var routeModes = routeModeProfile.ToModeList();

        var route = await _routingService.CreateMultiModalRouteAsync(DefaultOrigin, context.DestinationAddress, routeModes);
        var routeId = route.GetRouteId();
        var selectedTransportMode = ResolveSelectedTransportMode(route, routeModeProfile.MainTransportMode);
        var quoteInput = new RouteQuoteInput(
            context.HubId,
            context.Items
                .Select(item => new RouteQuoteItem(item.ProductId, item.Quantity, item.UnitWeightKg))
                .ToArray());
        var quote = _transportCarbonService.CalculateRouteQuote(
            route,
            quoteInput);

        var existingOptions = await _shippingOptionMapper.FindByOrderIdAsync(request.OrderId, cancellationToken);
        var option = existingOptions.FirstOrDefault() ?? new ShippingOption();

        option.ConfigureGeneratedOption(
            request.OrderId,
            routeId > 0 ? routeId : null,
            request.PreferenceType,
            profile.DisplayName,
            quote.Cost,
            quote.CarbonFootprintKg,
            profile.DeliveryDays,
            selectedTransportMode);

        if (existingOptions.Count > 0)
        {
            await _shippingOptionMapper.UpdateAsync(option, cancellationToken);
        }
        else
        {
            await _shippingOptionMapper.AddAsync(option, cancellationToken);
        }

        await _shippingOptionMapper.SaveChangesAsync(cancellationToken);

        var optionId = option.GetSummary().OptionId;
        await _shippingOptionMapper.SetCheckoutSelectedOptionAsync(checkoutId, optionId, cancellationToken);
        await _shippingOptionMapper.SaveChangesAsync(cancellationToken);

        return option.GetSelectionResult() with
        {
            OrderId = request.OrderId,
            DistanceKm = route.GetTotalDistanceKm() ?? 0d
        };
    }

    private static TransportMode ResolveSelectedTransportMode(DeliveryRoute route, TransportMode fallback)
    {
        var routeLegs = route.GetOrderedRouteLegs();
        var mainLegTransportMode = routeLegs
            .FirstOrDefault(routeLeg => routeLeg.GetIsMainTransport() == true)
            ?.GetTransportMode();

        if (mainLegTransportMode.HasValue)
        {
            return mainLegTransportMode.Value;
        }

        var firstNonTruckTransportMode = routeLegs
            .Select(routeLeg => routeLeg.GetTransportMode())
            .FirstOrDefault(transportMode => transportMode.HasValue && transportMode.Value != TransportMode.TRUCK);

        return firstNonTruckTransportMode ?? fallback;
    }

    private bool IsSameCountryRoute(OrderShippingContext context)
    {
        if (_transportationHubMapper is null)
        {
            return false;
        }

        var warehouseHub = _transportationHubMapper.FindById(context.HubId);
        return RouteCountryCodeResolver.TryResolveWarehouseCountryCode(warehouseHub, out var warehouseCountryCode) &&
               RouteCountryCodeResolver.TryResolveAddressCountryCode(context.DestinationAddress, out var destinationCountryCode) &&
               string.Equals(warehouseCountryCode, destinationCountryCode, StringComparison.OrdinalIgnoreCase);
    }

    private static PreferenceProfile GetPreferenceProfile(PreferenceType preferenceType)
    {
        return preferenceType switch
        {
            PreferenceType.FAST => new PreferenceProfile(
                "Fastest",
                "Best for time-sensitive deliveries when you want the quickest available route.",
                1,
                TransportMode.PLANE),
            PreferenceType.CHEAP => new PreferenceProfile(
                "Cheapest",
                "Best value when keeping delivery costs low matters more than speed.",
                5,
                TransportMode.SHIP),
            _ => new PreferenceProfile(
                "Greenest",
                "A balanced delivery choice that keeps the journey efficient while reducing transport impact.",
                4,
                TransportMode.TRAIN)
        };
    }

    private sealed record PreferenceProfile(
        string DisplayName,
        string Description,
        int DeliveryDays,
        TransportMode PrimaryTransportMode);
}
