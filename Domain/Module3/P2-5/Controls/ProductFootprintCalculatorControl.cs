using ProRental.Data.Module3.P2_5.Interfaces;
using ProRental.Domain.Module3.P2_5.Entities;
using ProRental.Domain.Module3.P2_5.Strategies;
using ProRental.Interfaces.Module2.P2_3;
using ProRental.Interfaces.Module3.P2_5;

namespace ProRental.Domain.Module3.P2_5.Controls;

public sealed class ProductFootprintCalculatorControl : IProductFootprintCalculatorService
{
    private readonly ICalculateCarbonStrategy<ProductFootprintInput> _productFootprintStrategy;
    private readonly IProductCatalogService _productCatalogService;
    private readonly IProductFootprintGateway _productFootprintGateway;

    public ProductFootprintCalculatorControl(
        ICalculateCarbonStrategy<ProductFootprintInput> productFootprintStrategy,
        IProductCatalogService productCatalogService,
        IProductFootprintGateway productFootprintGateway)
    {
        _productFootprintStrategy = productFootprintStrategy;
        _productCatalogService = productCatalogService;
        _productFootprintGateway = productFootprintGateway;
    }

    public double CalculateProductFootprint(double productMass, double toxicPercentage)
    {
        var input = new ProductFootprintInput(productMass, toxicPercentage);
        return _productFootprintStrategy.CalculateFootprint(input);
    }

    public List<ProductDropdownItem> GetProductDropdownItems()
    {
        return _productCatalogService.GetProductDropdownItems();
    }

    public List<ProductFootprintListItem> GetAllFootprints()
    {
        return _productFootprintGateway.GetAllFootprints();
    }

    public bool DeleteFootprint(int productCarbonFootprintId)
    {
        if (productCarbonFootprintId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(productCarbonFootprintId), "Invalid product footprint id.");
        }

        return _productFootprintGateway.DeleteFootprint(productCarbonFootprintId);
    }

    public ProductFootprintCalculationResult CalculateAndStoreFootprint(int productId, double productMass, double toxicPercentage)
    {
        if (!_productCatalogService.ProductExists(productId))
        {
            throw new InvalidOperationException($"Product {productId} was not found.");
        }

        var carbonFootprint = CalculateProductFootprint(productMass, toxicPercentage);
        return _productFootprintGateway.SaveCalculatedFootprint(productId, toxicPercentage, carbonFootprint);
    }
}
