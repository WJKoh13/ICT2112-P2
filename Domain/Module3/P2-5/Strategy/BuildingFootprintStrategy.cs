using ProRental.Interfaces.Module3;

namespace ProRental.Domain.Entities.Module3;

public class BuildingFootprintStrategy : ICalculateCarbonStrategy
{
    public double CalculateFootprint(params double[] values)
    {
        throw new NotImplementedException("Building footprint strategy has not been implemented yet.");
    }
}
