using ProRental.Domain.Entities;

namespace ProRental.Interfaces.Module2.P2_3;

public interface IProductQuery
{
    Product? GetProductById(int productId);
    decimal GetProductWeight(int productId);
    List<InventoryProductDropdownItem> GetAllProductDropdownItems();
    ProductStorageInfo? GetProductStorageInfo(int productId);
    List<ProductStorageInfo> GetAllProductStorageInfo();
}

/// <summary>
/// Lightweight DTO for product dropdowns (avoids needing public getters on Product entity).
/// </summary>
public class InventoryProductDropdownItem
{
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// DTO containing storage duration info for an individual inventory item.
/// </summary>
public class ProductStorageInfo
{
    public int InventoryItemId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime StoredSince { get; set; }
    public double HoursStored { get; set; }
}
