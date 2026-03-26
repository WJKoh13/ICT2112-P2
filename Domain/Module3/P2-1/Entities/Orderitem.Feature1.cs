namespace ProRental.Domain.Entities;

/// <summary>
/// Feature 1 accessors for the shared Orderitem entity. Exposes the product id
/// and quantity needed to build a shipping context from an order's line items.
/// by: ernest
/// </summary>
public partial class Orderitem
{
    public int GetProductId() => _productid;
    public int GetQuantity() => _quantity;
}
