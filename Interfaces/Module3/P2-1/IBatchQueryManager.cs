namespace ProRental.Interfaces.Module3.P2_1;

public interface IBatchQueryManager
{
    Task<IReadOnlyList<string>> GetBatches(CancellationToken cancellationToken = default);
}
