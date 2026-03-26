namespace ProRental.Domain.Module3.P2_5.Strategies;

public interface ICalculateCarbonStrategy<in TInput>
{
    double CalculateFootprint(TInput input);
}
