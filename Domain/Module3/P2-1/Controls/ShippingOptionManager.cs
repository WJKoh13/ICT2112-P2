using ProRental.Data.UnitOfWork;
using ProRental.Domain.Entities;
using ProRental.Domain.Enums;
using ProRental.Data.Interfaces;
using ProRental.Interfaces.Module3.P2_1;
using ProRental.Models.Module3.P2_1;

namespace ProRental.Domain.Controls;

/// <summary>
/// Feature 1 application service. It first exposes lightweight preference cards and
/// only generates a real route, quote, and persisted shipping option after selection.
/// by: ernest
/// </summary>
public sealed class ShippingOptionManager : IShippingOptionService
{
    private const string DefaultOrigin = "ProRental Warehouse";

    private readonly IShippingOptionMapper _shippingOptionMapper;
    private readonly ICheckoutShippingContextService _checkoutShippingContextService;
    private readonly IShippingPreferenceService _shippingPreferenceService;
    private readonly IRoutingService _routingService;
    private readonly ITransportCarbonService _transportCarbonService;
    private readonly ITransportationHubMapper? _transportationHubMapper;
    private readonly AppDbContext? _context;

    public ShippingOptionManager(
        IShippingOptionMapper shippingOptionMapper,
        ICheckoutShippingContextService checkoutShippingContextService,
        IShippingPreferenceService shippingPreferenceService,
        IRoutingService routingService,
        ITransportationHubMapper transportationHubMapper,
        ITransportCarbonService transportCarbonService,
        AppDbContext context)
    {
        _shippingOptionMapper = shippingOptionMapper;
        _checkoutShippingContextService = checkoutShippingContextService;
        _shippingPreferenceService = shippingPreferenceService;
        _routingService = routingService;
        _transportationHubMapper = transportationHubMapper;
        _transportCarbonService = transportCarbonService;
        _context = context;
    }

    internal ShippingOptionManager(
        IShippingOptionMapper shippingOptionMapper,
        ICheckoutShippingContextService checkoutShippingContextService,
        IShippingPreferenceService shippingPreferenceService,
        IRoutingService routingService,
        ITransportationHubMapper? transportationHubMapper,
        ITransportCarbonService transportCarbonService)
    {
        _shippingOptionMapper = shippingOptionMapper;
        _checkoutShippingContextService = checkoutShippingContextService;
        _shippingPreferenceService = shippingPreferenceService;
        _routingService = routingService;
        _transportationHubMapper = transportationHubMapper;
        _transportCarbonService = transportCarbonService;
        _context = null;
    }

    public async Task<IReadOnlyList<ShippingPreferenceCard>> GetPreferenceChoicesForCheckoutAsync(
        int checkoutId,
        CancellationToken cancellationToken = default)
    {
        var context = await _checkoutShippingContextService.GetShippingContextAsync(checkoutId, cancellationToken)
            ?? throw new InvalidOperationException($"Checkout '{checkoutId}' was not found.");

        return _shippingPreferenceService.BuildPreferenceCards(context, IsSameCountryRoute(context));
    }

    public async Task<ShippingSelectionResult> ConfirmPreferenceSelectionAsync(
        SelectShippingPreferenceRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.CheckoutId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(request.CheckoutId));
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
        var context = await _checkoutShippingContextService.GetShippingContextAsync(request.CheckoutId, cancellationToken)
            ?? throw new InvalidOperationException($"Checkout '{request.CheckoutId}' was not found.");
        var preference = _shippingPreferenceService.ResolvePreference(request.PreferenceType, IsSameCountryRoute(context));

        var route = await _routingService.CreateMultiModalRouteAsync(DefaultOrigin, context.DestinationAddress, [.. preference.AllowedModes]);
        var routeId = route.GetRouteId();
        var selectedTransportMode = ResolveSelectedTransportMode(route, preference.AllowedModes.First());
        var quoteInput = new RouteQuoteInput(
            context.HubId,
            context.Items
                .Select(item => new RouteQuoteItem(item.ProductId, item.Quantity, item.UnitWeightKg))
                .ToArray());
        var quote = _transportCarbonService.CalculateRouteQuote(
            route,
            quoteInput);

        var existingOptions = await _shippingOptionMapper.FindByCheckoutIdAsync(request.CheckoutId, cancellationToken);
        var option = existingOptions.FirstOrDefault() ?? new ShippingOption();

        option.ConfigureGeneratedOption(
            request.CheckoutId,
            routeId > 0 ? routeId : null,
            preference.PreferenceType,
            preference.DisplayName,
            quote.Cost,
            quote.CarbonFootprintKg,
            preference.DeliveryDays,
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
        await _shippingOptionMapper.SetCheckoutSelectedOptionAsync(request.CheckoutId, optionId, cancellationToken);
        await _shippingOptionMapper.SaveChangesAsync(cancellationToken);

        return option.GetSelectionResult() with
        {
            CheckoutId = request.CheckoutId,
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

    private bool IsSameCountryRoute(CheckoutShippingContext context)
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
}
