using Microsoft.EntityFrameworkCore;
using ProRental.Data.Interfaces;
using ProRental.Data.UnitOfWork;
using ProRental.Domain.Enums;
using ProRental.Interfaces;
using ProRental.Interfaces.Module3.P2_1;
using ProRental.Models.Module3.P2_1;

namespace ProRental.Domain.Controls;

/// <summary>
/// Temporary Module 1 adapter used by Feature 1 while the final checkout-side
/// integration contract is still being built. It exposes only the order inputs
/// needed to construct a shipping option set.
/// by: ernest
/// </summary>
public sealed class ShippingOrderContextService : IOrderService
{
    private readonly AppDbContext _context;
    private readonly IInventoryService _inventoryService;
    private readonly ITransportationHubMapper _transportationHubMapper;

    public ShippingOrderContextService(
        AppDbContext context,
        IInventoryService inventoryService,
        ITransportationHubMapper transportationHubMapper)
    {
        _context = context;
        _inventoryService = inventoryService;
        _transportationHubMapper = transportationHubMapper;
    }

    public async Task<OrderShippingContext?> GetShippingContextAsync(
        int orderId,
        CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders
            .Include(entity => entity.Customer)
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => EF.Property<int>(entity, "Orderid") == orderId, cancellationToken);

        if (order is null)
        {
            return null;
        }

        var destinationAddress = order.Customer.GetAddress();
        if (string.IsNullOrWhiteSpace(destinationAddress))
        {
            throw new InvalidOperationException($"Order '{orderId}' does not have a delivery address.");
        }

        var orderContext = order.GetOrderContext();
        var orderItems = await _context.Orderitems
            .AsNoTracking()
            .Where(entity => EF.Property<int>(entity, "Orderid") == orderId)
            .Select(entity => new
            {
                ProductId = EF.Property<int>(entity, "Productid"),
                Quantity = EF.Property<int>(entity, "Quantity")
            })
            .ToListAsync(cancellationToken);

        if (orderItems.Count == 0)
        {
            throw new InvalidOperationException($"Order '{orderId}' does not contain any order items.");
        }

        var warehouseHub = _transportationHubMapper.FindByType(HubType.WAREHOUSE)
            .FirstOrDefault(hub => hub.GetHubId() > 0)
            ?? throw new InvalidOperationException("No warehouse hub is configured for shipping route generation.");

        var items = orderItems
            .GroupBy(orderItem => orderItem.ProductId)
            .Select(group =>
            {
                var unitWeightKg = (double)_inventoryService.GetProductWeight(group.Key);
                return new OrderShippingItem(
                    group.Key,
                    group.Sum(orderItem => orderItem.Quantity),
                    unitWeightKg);
            })
            .OrderBy(item => item.ProductId)
            .ToArray();

        var totalShipmentWeightKg = items.Sum(item => item.Quantity * item.UnitWeightKg);

        return new OrderShippingContext(
            orderContext.OrderId,
            orderContext.CustomerId,
            orderContext.CheckoutId,
            destinationAddress,
            warehouseHub.GetHubId(),
            items,
            totalShipmentWeightKg);
    }
}
