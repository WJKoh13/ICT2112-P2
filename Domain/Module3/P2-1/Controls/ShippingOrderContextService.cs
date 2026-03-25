using Microsoft.EntityFrameworkCore;
using ProRental.Data.UnitOfWork;
using ProRental.Domain.Enums;
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

    public ShippingOrderContextService(AppDbContext context)
    {
        _context = context;
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

        return new OrderShippingContext(
            orderContext.OrderId,
            orderContext.CustomerId,
            orderContext.CheckoutId,
            destinationAddress,
            ProductId: 1,
            HubId: 1,
            // Placeholder values: the final cross-module contract is expected to source
            // product, hub, weight, and quantity from Module 1 / Module 2 instead of hardcoding them here.
            WeightKg: 1d,
            Quantity: 1); //hardcoded as 1 for both
    }

    public async Task<IReadOnlyList<OrderItemDetail>> OrderDetails(
        int orderId,
        CancellationToken cancellationToken = default)
    {
        var items = await _context.Orderitems
            .AsNoTracking()
            .Where(entity => EF.Property<int>(entity, "Orderid") == orderId)
            .Select(entity => new OrderItemDetail(
                EF.Property<int>(entity, "Orderitemid"),
                EF.Property<int>(entity, "Productid"),
                EF.Property<int>(entity, "Quantity"),
                (double)EF.Property<decimal>(entity, "Unitprice"),
                1d))
            .ToListAsync(cancellationToken);

        return items;
    }

    public async Task<int> GetOrderItemQuantity(
        int orderId,
        int productId,
        CancellationToken cancellationToken = default)
    {
        var quantity = await _context.Orderitems
            .AsNoTracking()
            .Where(entity =>
                EF.Property<int>(entity, "Orderid") == orderId &&
                EF.Property<int>(entity, "Productid") == productId)
            .SumAsync(entity => EF.Property<int>(entity, "Quantity"), cancellationToken);

        return quantity;
    }

    public async Task<string?> GetDeliveryAddress(
        int orderId,
        CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders
            .Include(entity => entity.Customer)
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => EF.Property<int>(entity, "Orderid") == orderId, cancellationToken);

        return order?.Customer.GetAddress();
    }

    public async Task<bool> IsOrderDispatchedAsync(
        int orderId,
        CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => EF.Property<int>(entity, "Orderid") == orderId, cancellationToken);

        if (order is null)
        {
            return false;
        }

        var status = EF.Property<OrderStatus?>(order, "Status");
        return status == OrderStatus.DISPATCHED;
    }
}
