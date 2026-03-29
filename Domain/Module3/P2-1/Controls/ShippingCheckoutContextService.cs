using Microsoft.EntityFrameworkCore;
using ProRental.Data.Interfaces;
using ProRental.Data.UnitOfWork;
using ProRental.Domain.Enums;
using ProRental.Interfaces;
using ProRental.Interfaces.Module2.P2_3;
using ProRental.Interfaces.Module3.P2_1;
using ProRental.Models.Module3.P2_1;

namespace ProRental.Domain.Controls;

/// <summary>
/// Checkout-backed Module 1 adapter used by Feature 1. It exposes only the checkout
/// inputs needed to construct a shipping option set.
/// by: ernest
/// </summary>
public sealed class ShippingCheckoutContextService : ICheckoutShippingContextService
{
    private readonly AppDbContext _context;
    private readonly IProductQuery _productQuery;
    private readonly ITransportationHubMapper _transportationHubMapper;

    public ShippingCheckoutContextService(
        AppDbContext context,
        IProductQuery productQuery,
        ITransportationHubMapper transportationHubMapper)
    {
        _context = context;
        _productQuery = productQuery;
        _transportationHubMapper = transportationHubMapper;
    }

    public async Task<CheckoutShippingContext?> GetShippingContextAsync(
        int checkoutId,
        CancellationToken cancellationToken = default)
    {
        var checkout = await _context.Checkouts
            .Include(entity => entity.Customer)
            .Include(entity => entity.Cart)
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => EF.Property<int>(entity, "Checkoutid") == checkoutId, cancellationToken);

        if (checkout is null)
        {
            return null;
        }

        var destinationAddress = checkout.Customer.GetAddress();
        if (string.IsNullOrWhiteSpace(destinationAddress))
        {
            throw new InvalidOperationException($"Checkout '{checkoutId}' does not have a delivery address.");
        }

        var checkoutContext = checkout.GetCheckoutContext();
        var selectedCartItems = await _context.Cartitems
            .AsNoTracking()
            .Where(entity =>
                EF.Property<int>(entity, "Cartid") == checkoutContext.CartId &&
                EF.Property<bool?>(entity, "Isselected") == true)
            .Select(entity => new
            {
                ProductId = EF.Property<int>(entity, "Productid"),
                Quantity = EF.Property<int>(entity, "Quantity")
            })
            .ToListAsync(cancellationToken);

        if (selectedCartItems.Count == 0)
        {
            throw new InvalidOperationException($"Checkout '{checkoutId}' does not contain any selected cart items.");
        }

        var warehouseHub = _transportationHubMapper.FindByType(HubType.WAREHOUSE)
            .FirstOrDefault(hub => hub.GetHubId() > 0)
            ?? throw new InvalidOperationException("No warehouse hub is configured for shipping route generation.");

        var items = selectedCartItems
            .GroupBy(cartItem => cartItem.ProductId)
            .Select(group =>
            {
                var unitWeightKg = (double)_productQuery.GetProductWeight(group.Key);
                return new CheckoutShippingItem(
                    group.Key,
                    group.Sum(cartItem => cartItem.Quantity),
                    unitWeightKg);
            })
            .OrderBy(item => item.ProductId)
            .ToArray();

        var totalShipmentWeightKg = items.Sum(item => item.Quantity * item.UnitWeightKg);

        return new CheckoutShippingContext(
            checkoutId,
            checkoutContext.CustomerId,
            destinationAddress,
            warehouseHub.GetHubId(),
            items,
            totalShipmentWeightKg);
    }
}
