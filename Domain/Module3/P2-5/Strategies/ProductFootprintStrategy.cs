namespace ProRental.Domain.Module3.P2_5.Strategies;

public sealed class ProductFootprintStrategy : ICalculateCarbonStrategy<ProductFootprintInput>
{
    public double CalculateFootprint(ProductFootprintInput input)
    {
        if (input.ProductMass < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(input.ProductMass), "Product mass cannot be negative.");
        }

        if (input.ToxicPercentage < 0 || input.ToxicPercentage > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(input.ToxicPercentage), "Toxic percentage must be between 0 and 100.");
        }

        return Math.Round(input.ProductMass * 0.5 * (1 + (input.ToxicPercentage / 100.0)), 2);
    }
}
