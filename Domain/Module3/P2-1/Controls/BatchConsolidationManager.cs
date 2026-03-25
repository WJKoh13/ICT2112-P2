using ProRental.Data.Module3.P2_1.Interfaces;
using ProRental.Domain.Entities;
using ProRental.Domain.Enums;
using ProRental.Interfaces.Module3.P2_1;

namespace ProRental.Domain.Module3.P2_1.Controls;

public sealed class BatchConsolidationManager : IBatchDelivery
{
    private const int FirstLegRouteId = 1;

    private readonly IBatchValidator _batchValidator;
    private readonly IOrderService _orderService;
    private readonly IRouteQueryService _routeQueryService;
    private readonly ITransportCarbonService _transportCarbonService;
    private readonly IHubInfoService _hubInfoService;
    private readonly IDeliveryBatchMapper _deliveryBatchMapper;
    private readonly IBatchOrderMapper _batchOrderMapper;

    public BatchConsolidationManager(
        IBatchValidator batchValidator,
        IOrderService orderService,
        IRouteQueryService routeQueryService,
        ITransportCarbonService transportCarbonService,
        IHubInfoService hubInfoService,
        IDeliveryBatchMapper deliveryBatchMapper,
        IBatchOrderMapper batchOrderMapper)
    {
        _batchValidator = batchValidator;
        _orderService = orderService;
        _routeQueryService = routeQueryService;
        _transportCarbonService = transportCarbonService;
        _hubInfoService = hubInfoService;
        _deliveryBatchMapper = deliveryBatchMapper;
        _batchOrderMapper = batchOrderMapper;
    }

    public async Task ConsolidateOrderToBatch(string orderId, CancellationToken cancellationToken = default)
    {
        if (!int.TryParse(orderId, out var parsedOrderId))
        {
            HandleConsolidationFailure(orderId, "Order id must be a valid integer.");
        }

        var orderExists = await _batchValidator.ValidateOrderExists(parsedOrderId, cancellationToken);
        if (!orderExists)
        {
            HandleConsolidationFailure(orderId, "Order was not found.");
        }

        if (await _batchOrderMapper.OrderAlreadyAssigned(parsedOrderId, cancellationToken))
        {
            HandleConsolidationFailure(orderId, "Order is already assigned to a delivery batch.");
        }

        var isDispatched = await _orderService.IsOrderDispatchedAsync(parsedOrderId, cancellationToken);
        if (isDispatched)
        {
            HandleConsolidationFailure(orderId, "Order has already been dispatched and cannot be batched.");
        }

        var routeLeg = await _routeQueryService.RetrieveFirstMileLeg(FirstLegRouteId, cancellationToken);
        if (routeLeg is null)
        {
            HandleConsolidationFailure(orderId, "No first-mile route leg was found for route id 1.");
        }

        await BatchOrderConsolidator(orderId, routeLeg.GetEndPoint(), cancellationToken);
    }

    public async Task<bool> MarkBatchesAsShipped(IReadOnlyList<string> batchIds, CancellationToken cancellationToken = default)
    {
        var markedAny = false;

        foreach (var batchIdRaw in batchIds)
        {
            if (!int.TryParse(batchIdRaw, out var batchId))
            {
                continue;
            }

            var batch = await _deliveryBatchMapper.FindByIdentifier(batchId, cancellationToken);
            if (batch is null || batch.GetDeliveryBatchStatus() == BatchStatus.SHIPPEDOUT)
            {
                continue;
            }

            batch.MarkAsShipped();
            await _deliveryBatchMapper.Update(batch, cancellationToken);
            markedAny = true;
        }

        return markedAny;
    }

    public async Task<bool> BatchOrderConsolidator(string orderId, string destinationHub, CancellationToken cancellationToken = default)
    {
        if (!int.TryParse(orderId, out var parsedOrderId))
        {
            HandleConsolidationFailure(orderId, "Order id must be a valid integer.");
        }

        var destinationHubId = ResolveDestinationHubId(destinationHub);
        if (destinationHubId <= 0)
        {
            HandleConsolidationFailure(orderId, $"Unable to resolve destination hub from '{destinationHub}'.");
        }

        var hubInfo = _hubInfoService.GetHubInfo(destinationHubId);
        if (hubInfo is null)
        {
            HandleConsolidationFailure(orderId, $"Hub '{destinationHubId}' was not found.");
        }

        if (hubInfo.GetHubType() != HubType.WAREHOUSE && hubInfo is not Warehouse)
        {
            HandleConsolidationFailure(orderId, $"Hub '{destinationHubId}' is not a warehouse.");
        }

        var orderAddress = await _orderService.GetDeliveryAddress(parsedOrderId, cancellationToken);
        if (string.IsNullOrWhiteSpace(orderAddress))
        {
            HandleConsolidationFailure(orderId, "Order does not have a destination address.");
        }

        var pendingBatches = await _deliveryBatchMapper.GetBatchByStatus(destinationHubId, BatchStatus.PENDING, cancellationToken);
        var targetBatch = pendingBatches
            .FirstOrDefault(batch =>
                string.Equals(batch.GetDestinationAddress(), orderAddress, StringComparison.OrdinalIgnoreCase));

        if (targetBatch is null)
        {
            targetBatch = CreateNewBatch(destinationHubId, hubInfo.GetAddress());
            await _deliveryBatchMapper.Insert(targetBatch, cancellationToken);
        }

        var added = await _batchOrderMapper.AddOrderToBatch(targetBatch.GetDeliveryBatchIdentifier(), parsedOrderId, cancellationToken);
        if (!added)
        {
            return false;
        }

        var allOrderIds = await _batchOrderMapper.GetOrderIdsByBatch(targetBatch.GetDeliveryBatchIdentifier(), cancellationToken);
        var routeLeg = await _routeQueryService.RetrieveFirstMileLeg(FirstLegRouteId, cancellationToken);
        var distanceKm = routeLeg?.GetDistanceKm() ?? 0d;

        var batchWeight = await CalculateBatchWeight(allOrderIds, cancellationToken);
        targetBatch.UpdateBatchWeight(batchWeight);
        targetBatch.UpdateCarbonSavings(await BatchCarbonCostSavings(allOrderIds, distanceKm, cancellationToken));
        await _deliveryBatchMapper.Update(targetBatch, cancellationToken);

        return true;
    }

    public void HandleConsolidationFailure(string orderId, string reason)
    {
        throw new InvalidOperationException($"Batch consolidation failed for order '{orderId}'. Reason: {reason}");
    }

    public async Task<double> BatchCarbonCostSavings(
        IReadOnlyList<int> consolidatedOrderIds,
        double distanceKm,
        CancellationToken cancellationToken = default)
    {
        if (consolidatedOrderIds.Count <= 1)
        {
            return 0d;
        }

        var unconsolidatedCost = await CalculateUnconsolidatedFirstLegCost(consolidatedOrderIds, distanceKm, cancellationToken);
        var batchWeight = await CalculateBatchWeight(consolidatedOrderIds, cancellationToken);
        var consolidatedCost = _transportCarbonService.CalculateLegCarbon(1, Math.Max(batchWeight, 1d), distanceKm, 0d);

        var savings = unconsolidatedCost - consolidatedCost;
        return savings > 0d ? Math.Round(savings, 2, MidpointRounding.AwayFromZero) : 0d;
    }

    public DeliveryBatch CreateNewBatch(int destinationHubId, string destinationAddress)
    {
        return _deliveryBatchMapper.CreateNewBatch(destinationHubId, destinationAddress);
    }

    public async Task<double> CalculateUnconsolidatedFirstLegCost(
        IEnumerable<int> orderIds,
        double distanceKm,
        CancellationToken cancellationToken = default)
    {
        double total = 0d;

        foreach (var orderId in orderIds)
        {
            var orderItems = await _orderService.OrderDetails(orderId, cancellationToken);
            var weight = orderItems.Sum(item => Math.Max(item.WeightKg, 0d) * Math.Max(item.Quantity, 0));
            total += _transportCarbonService.CalculateLegCarbon(1, Math.Max(weight, 1d), distanceKm, 0d);
        }

        return total;
    }

    public async Task<double> CalculateBatchWeight(
        IEnumerable<int> orderIds,
        CancellationToken cancellationToken = default)
    {
        double totalWeight = 0d;

        foreach (var orderId in orderIds)
        {
            var orderItems = await _orderService.OrderDetails(orderId, cancellationToken);
            totalWeight += orderItems.Sum(item => Math.Max(item.WeightKg, 0d) * Math.Max(item.Quantity, 0));
        }

        return Math.Round(totalWeight, 2, MidpointRounding.AwayFromZero);
    }

    private int ResolveDestinationHubId(string destinationHub)
    {
        if (int.TryParse(destinationHub, out var destinationHubId))
        {
            return destinationHubId;
        }

        var hubs = _hubInfoService.GetAllHubs();
        var matched = hubs.FirstOrDefault(hub =>
            string.Equals(hub.GetAddress(), destinationHub, StringComparison.OrdinalIgnoreCase));

        return matched?.GetHubId() ?? 0;
    }
}
