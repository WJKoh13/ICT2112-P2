namespace ProRental.Interfaces.Module3.P2_1;

public interface IGoogleMapsAPI
{
    Task<double> FetchRouteDistanceKmAsync(
        string origin,
        string destination,
        CancellationToken cancellationToken = default);
}
