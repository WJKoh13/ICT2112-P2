using Microsoft.EntityFrameworkCore;
using ProRental.Data.UnitOfWork;
using ProRental.Domain.Entities;
using ProRental.Interfaces.Module2.P2_3;

namespace ProRental.Domain.Module2.P2_3.Controls;

public class InventoryService : IInventoryService
{
    private readonly AppDbContext _context;

    public InventoryService(AppDbContext context)
    {
        _context = context;
    }

    public Product? GetProductById(int productId)
    {
        return _context.Products
            .Include(p => p.Productdetail)
            .FirstOrDefault(p => EF.Property<int>(p, "Productid") == productId);
    }

    public List<InventoryProductDropdownItem> GetAllProductDropdownItems()
    {
        return _context.Set<Productdetail>()
            .Select(pd => new InventoryProductDropdownItem
            {
                ProductId = EF.Property<int>(pd, "Productid"),
                Name = EF.Property<string>(pd, "Name")
            })
            .ToList();
    }

    public decimal GetProductWeight(int productId)
    {
        var product = GetProductById(productId);
        if (product?.Productdetail == null) return 0;

        var weight = _context.Set<Productdetail>()
            .Where(pd => EF.Property<int>(pd, "Productid") == productId)
            .Select(pd => EF.Property<decimal?>(pd, "Weight"))
            .FirstOrDefault();

        return weight ?? 0;
    }

    public List<ProductStorageInfo> GetAllProductStorageInfo()
    {
        var now = DateTime.UtcNow;

        var inventoryItems = _context.Set<Inventoryitem>()
            .Select(i => new
            {
                InventoryItemId = EF.Property<int>(i, "Inventoryid"),
                ProductId = EF.Property<int>(i, "Productid"),
                SerialNumber = EF.Property<string>(i, "Serialnumber"),
                CreatedAt = EF.Property<DateTime>(i, "Createdat")
            })
            .ToList();

        var productDetails = _context.Set<Productdetail>()
            .Select(pd => new
            {
                ProductId = EF.Property<int>(pd, "Productid"),
                Name = EF.Property<string>(pd, "Name")
            })
            .ToList();

        var results = new List<ProductStorageInfo>();
        foreach (var inv in inventoryItems)
        {
            var detail = productDetails.FirstOrDefault(d => d.ProductId == inv.ProductId);
            var hoursStored = (now - inv.CreatedAt).TotalHours;

            results.Add(new ProductStorageInfo
            {
                InventoryItemId = inv.InventoryItemId,
                ProductId = inv.ProductId,
                ProductName = detail?.Name ?? "Unknown",
                SerialNumber = inv.SerialNumber,
                Quantity = 1,
                StoredSince = inv.CreatedAt,
                HoursStored = hoursStored > 0 ? hoursStored : 0
            });
        }

        return results;
    }

    public ProductStorageInfo? GetProductStorageInfo(int productId)
    {
        var inventoryData = _context.Set<Inventoryitem>()
            .Where(i => EF.Property<int>(i, "Productid") == productId)
            .Select(i => new
            {
                InventoryItemId = EF.Property<int>(i, "Inventoryid"),
                SerialNumber = EF.Property<string>(i, "Serialnumber"),
                CreatedAt = EF.Property<DateTime>(i, "Createdat")
            })
            .OrderBy(i => i.CreatedAt)
            .FirstOrDefault();

        if (inventoryData == null) return null;

        var productName = _context.Set<Productdetail>()
            .Where(pd => EF.Property<int>(pd, "Productid") == productId)
            .Select(pd => EF.Property<string>(pd, "Name"))
            .FirstOrDefault() ?? "Unknown";

        var quantity = _context.Set<Inventoryitem>()
            .Where(i => EF.Property<int>(i, "Productid") == productId)
            .Count();

        var now = DateTime.UtcNow;
        var hoursStored = (now - inventoryData.CreatedAt).TotalHours;

        return new ProductStorageInfo
        {
            InventoryItemId = inventoryData.InventoryItemId,
            ProductId = productId,
            ProductName = productName,
            SerialNumber = inventoryData.SerialNumber,
            Quantity = quantity,
            StoredSince = inventoryData.CreatedAt,
            HoursStored = hoursStored > 0 ? hoursStored : 0
        };
    }
}
