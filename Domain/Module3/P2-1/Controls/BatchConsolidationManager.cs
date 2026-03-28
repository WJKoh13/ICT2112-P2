using ProRental.Data.Module3.P2_1.Interfaces;
using ProRental.Domain.Entities;
using ProRental.Domain.Enums;
using ProRental.Interfaces.Module3.P2_1;

namespace ProRental.Domain.Module3.P2_1.Controls;

public sealed class BatchConsolidationManager : IBatchDelivery
{
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

    public void consolidateOrderToBatch(string orderId)
    {
        if (!_batchValidator.validateOrderExists(orderId))
        {
            handleConsolidationFailure(orderId, "Order does not exist.");
        }

        if (_batchOrderMapper.orderAlreadyAssigned(orderId))
        {
            handleConsolidationFailure(orderId, "Order is already assigned to a batch.");
        }

        var shippingContext = _orderService.GetShippingContextAsync(int.Parse(orderId)).GetAwaiter().GetResult();
        if (shippingContext is null || string.IsNullOrWhiteSpace(shippingContext.DestinationAddress))
        {
            handleConsolidationFailure(orderId, "Order does not have a valid delivery address.");
        }

        var routeId = 2; // FR assumption: fixed first-leg/main transport route ID.
        var mainTransportLeg = _routeQueryService.retrieveMainTransportLeg(routeId)
            ?? throw new InvalidOperationException($"No main transport route leg was found for route ID '{routeId}'.");

        var destinationHubEndpoint = mainTransportLeg.GetEndPoint();
        var destinationHubId = int.TryParse(destinationHubEndpoint, out var parsedDestinationHubId)
            ? parsedDestinationHubId
            : _hubInfoService.GetAllHubs()
                .FirstOrDefault(hub => string.Equals(hub.GetAddress(), destinationHubEndpoint, StringComparison.OrdinalIgnoreCase))
                ?.GetHubId() ?? 0;

        if (destinationHubId <= 0)
        {
            handleConsolidationFailure(orderId, "Destination hub is invalid.");
        }

        if (!batchOrderConsolidator(orderId, destinationHubId.ToString()))
        {
            handleConsolidationFailure(orderId, "Unable to add order to pending batch.");
        }
    }

    public bool batchOrderConsolidator(string orderId, string destHub)
    {
        var pendingBatches = _deliveryBatchMapper.getBatchByStatus(destHub, BatchStatus.PENDING);
        var targetBatch = pendingBatches.FirstOrDefault();

        if (targetBatch is null)
        {
            if (!int.TryParse(destHub, out var destinationHubId))
            {
                handleConsolidationFailure(orderId, "Destination hub is invalid.");
            }

            var hub = _hubInfoService.GetHubInfo(destinationHubId);
            if (hub is null)
            {
                throw new InvalidOperationException($"Consolidation failed for order '{orderId}': destination hub could not be resolved.");
            }

            targetBatch = createNewBatch(destinationHubId, hub.GetAddress());
        }

        var wasAdded = _batchOrderMapper.addOrderToBatch(targetBatch.GetDeliveryBatchIdentifier(), orderId);
        if (!wasAdded)
        {
            return false;
        }

        RecalculateBatchMetrics(targetBatch.GetDeliveryBatchIdentifier());
        return true;
    }

    public void handleConsolidationFailure(string orderId, string reason)
    {
        throw new InvalidOperationException($"Consolidation failed for order '{orderId}': {reason}");
    }

    public double batchCarbonCostSavings(double unconsolidatedCost, List<string> consOrderIds)
    {
        if (consOrderIds.Count <= 1)
        {
            return 0d;
        }

        var distanceKm = _routeQueryService.retrieveMainTransportLeg(2)?.GetDistanceKm() ?? 0d; // FR assumption: fixed first-leg/main transport route ID.
        var batchWeightKg = CalculateBatchWeight(consOrderIds);

        var consolidatedLegCost = _transportCarbonService.CalculateLegCarbon(
            quantity: 1,
            weightKg: batchWeightKg,
            distanceKm: distanceKm,
            storageCo2: 0d);

        return Math.Max(0d, unconsolidatedCost - consolidatedLegCost);
    }

    public DeliveryBatch createNewBatch(int destHubID, string destinationAddress)
    {
        var newBatch = _deliveryBatchMapper.createNewBatch(destHubID, destinationAddress);
        _deliveryBatchMapper.insert(newBatch);
        return newBatch;
    }

    public bool markBatchesAsShipped(List<string> batchIds)
    {
        foreach (var batchId in batchIds)
        {
            if (!int.TryParse(batchId, out var parsedBatchId))
            {
                return false;
            }

            if (!_batchValidator.validateBatchExists(parsedBatchId))
            {
                return false;
            }

            var batch = _deliveryBatchMapper.findByIdentifier(batchId);
            if (batch is null)
            {
                return false;
            }

            batch.markAsShipped();
            _deliveryBatchMapper.update(batch);
        }

        return true;
    }

    public bool resetOrderBatchAssignments()
    {
        // Admin/demo utility kept for existing controller + interface flow; not part of core consolidation operations in the design diagram.
        var wasCleared = _batchOrderMapper.clearAllOrderBatchLinks();
        if (!wasCleared)
        {
            return false;
        }

        var batches = _deliveryBatchMapper.findAll();
        foreach (var batch in batches)
        {
            batch.SetTotalOrders(0);
            batch.updateBatchWeight(0d);
            batch.updateCarbonSavings(0d);
            _deliveryBatchMapper.update(batch);
        }

        return true;
    }

    public double CalculateUnconsolidatedFirstLegCost(IEnumerable<string> orderIds, double distanceKm)
    {
        var total = 0d;

        foreach (var orderId in orderIds)
        {
            if (!int.TryParse(orderId, out var parsedOrderId))
            {
                continue;
            }

            var orderWeightKg = GetOrderWeightKg(parsedOrderId);
            total += _transportCarbonService.CalculateLegCarbon(
                quantity: 1,
                weightKg: orderWeightKg,
                distanceKm: distanceKm,
                storageCo2: 0d);
        }

        return total;
    }

    public double CalculateBatchWeight(IEnumerable<string> orderIds)
    {
        var totalWeight = 0d;

        foreach (var orderId in orderIds)
        {
            if (!int.TryParse(orderId, out var parsedOrderId))
            {
                continue;
            }

            totalWeight += GetOrderWeightKg(parsedOrderId);
        }

        return Math.Max(totalWeight, 1d);
    }

    public void RecalculateBatchMetrics(int batchId)
    {
        var batch = _deliveryBatchMapper.findByIdentifier(batchId.ToString());
        if (batch is null)
        {
            return;
        }

        var orderIds = _batchOrderMapper.getOrderIdsByBatch(batchId);
        var totalOrders = _batchOrderMapper.countOrdersInBatch(batchId);
        var distanceKm = _routeQueryService.retrieveMainTransportLeg(2)?.GetDistanceKm() ?? 0d; // FR assumption: fixed first-leg/main transport route ID.

        var batchWeightKg = CalculateBatchWeight(orderIds);
        var unconsolidatedCost = CalculateUnconsolidatedFirstLegCost(orderIds, distanceKm);
        var carbonSavings = batchCarbonCostSavings(unconsolidatedCost, orderIds);

        batch.SetTotalOrders(totalOrders);
        batch.updateBatchWeight(batchWeightKg);
        batch.updateCarbonSavings(carbonSavings);
        _deliveryBatchMapper.update(batch);
    }

    public double GetOrderWeightKg(int orderId)
    {
        return _batchOrderMapper.getOrderWeightKg(orderId);
    }
}
