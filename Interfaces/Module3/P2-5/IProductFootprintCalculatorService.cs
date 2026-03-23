using ProRental.Domain.Module3.P2_5.Entities;

namespace ProRental.Interfaces.Module3.P2_5;

public interface IProductFootprintCalculatorService
{
    double CalculateProductFootprint(double productMass, double toxicPercentage);
    List<ProductDropdownItem> GetProductDropdownItems();
    ProductFootprintCalculationResult CalculateAndStoreFootprint(int productId, double productMass, double toxicPercentage);
}
