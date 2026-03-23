using ProRental.Domain.Entities;
using ProRental.Domain.Enums;
using ProRental.Interfaces.Module3.P2_1;
using ProRental.Models.Module3.P2_1;

namespace ProRental.Domain.Controls;

/// <summary>
/// Feature 1 application service. It orchestrates option generation, delegates route
/// and quote construction to dependent services, and persists the customer's choice.
/// by: ernest
/// </summary>
public sealed class ShippingOptionManager : IShippingOptionService
{
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

    public ShippingOptionManager(
        IShippingOptionMapper shippingOptionMapper,
        IOrderService orderService,
        IRoutingService routingService,
        ITransportCarbonService transportCarbonService)
    {
        _shippingOptionMapper = shippingOptionMapper;
        _orderService = orderService;
        _routingService = routingService;
        _transportCarbonService = transportCarbonService;
    }

    public async Task<IReadOnlyList<ShippingOptionSummary>> GetShippingOptionsForOrderAsync(
        int orderId,
        CancellationToken cancellationToken = default)
    {
        // Reuse persisted options when present so checkout sees stable values across reloads.
        var existingOptions = await _shippingOptionMapper.FindByOrderIdAsync(orderId, cancellationToken);
        if (existingOptions.Count > 0)
        {
            return existingOptions.Select(ToSummary).ToArray();
        }

        var context = await _orderService.GetShippingContextAsync(orderId, cancellationToken)
            ?? throw new InvalidOperationException($"Order '{orderId}' was not found.");

        return await BuildOptionSetAsync(context, cancellationToken);
    }

    public async Task<IReadOnlyList<ShippingOptionSummary>> BuildOptionSetAsync(
        OrderShippingContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        // Feature 1 owns the three customer-facing preference buckets even though route
        // construction and carbon pricing are delegated to other module contracts.
        var options = new List<ShippingOption>();

        foreach (var preferenceType in PreferenceOrder)
        {
            var route = await _routingService.CreateRouteAsync(
                new RoutingRequest(
                    context.OrderId,
                    context.DestinationAddress,
                    context.WeightKg,
                    context.Quantity,
                    preferenceType),
                cancellationToken);

            var quote = await _transportCarbonService.QuoteAsync(
                route,
                context,
                preferenceType,
                cancellationToken);

            var option = new ShippingOption();
            var routeId = route.GetRouteId();
            option.ConfigureGeneratedOption(
                context.OrderId,
                routeId > 0 ? routeId : null,
                preferenceType,
                string.IsNullOrWhiteSpace(quote.DisplayName) ? preferenceType.ToString() : quote.DisplayName,
                quote.Cost,
                quote.CarbonFootprintKg,
                quote.DeliveryDays,
                quote.TransportMode);

            options.Add(option);
        }

        await _shippingOptionMapper.AddRangeAsync(options, cancellationToken);
        await _shippingOptionMapper.SaveChangesAsync(cancellationToken);

        return options.Select(ToSummary).ToArray();
    }

    public async Task<ShippingSelectionResult> ApplyCustomerSelectionAsync(
        SelectShippingOptionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.OrderId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(request.OrderId));
        }

        if (request.OptionId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(request.OptionId));
        }

        var selectedOption = await _shippingOptionMapper.FindByIdAsync(request.OptionId, cancellationToken)
            ?? throw new InvalidOperationException($"Shipping option '{request.OptionId}' was not found.");

        if (!selectedOption.BelongsToOrder(request.OrderId))
        {
            throw new InvalidOperationException(
                $"Shipping option '{request.OptionId}' does not belong to order '{request.OrderId}'.");
        }

        var order = await _shippingOptionMapper.FindOrderWithCheckoutAsync(request.OrderId, cancellationToken)
            ?? throw new InvalidOperationException($"Order '{request.OrderId}' was not found.");

        var checkoutId = order.GetOrderContext().CheckoutId;
        if (checkoutId <= 0)
        {
            throw new InvalidOperationException($"Order '{request.OrderId}' does not have a checkout record.");
        }

        // The stored checkout selection remains the source of truth for Module 1 integration.
        var selection = selectedOption.GetSelectionResult();

        await _shippingOptionMapper.SetCheckoutSelectedOptionAsync(checkoutId, selection.OptionId, cancellationToken);
        await _shippingOptionMapper.SaveChangesAsync(cancellationToken);

        return selection with { OrderId = request.OrderId };
    }

    private static ShippingOptionSummary ToSummary(ShippingOption option)
    {
        return option.GetSummary();
    }
}
