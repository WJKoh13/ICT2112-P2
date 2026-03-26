using Microsoft.EntityFrameworkCore;
using ProRental.Data.UnitOfWork;
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
            .Include(entity => entity.Orderitems)
                .ThenInclude(item => item.Product)
                    .ThenInclude(product => product.Productdetail)
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
        var firstItem = order.Orderitems.FirstOrDefault();
        var productId = firstItem?.GetProductId() ?? 1;
        var quantity = order.Orderitems.Sum(item => item.GetQuantity());
        var weightKg = firstItem?.Product.Productdetail?.GetWeightKg() ?? 1d;

        // HubId: no direct order→warehouse FK exists yet. Falls back to the first
        // warehouse hub in the database until Module 2 provides the stored-at hub.
        var hubId = await _context.TransportationHubs
            .AsNoTracking()
            .Select(hub => (int?)EF.Property<int>(hub, "HubId"))
            .FirstOrDefaultAsync(cancellationToken) ?? 1;

        return new OrderShippingContext(
            orderContext.OrderId,
            orderContext.CustomerId,
            orderContext.CheckoutId,
            destinationAddress,
            ProductId: productId,
            HubId: hubId,
            WeightKg: weightKg,
            Quantity: quantity > 0 ? quantity : 1);
    }
}
