namespace ProRental.Interfaces.Module3.P2_1;

public interface IGoogleMapsApi
{
    Task<double?> FetchRouteDistanceKmAsync(
        string origin,
        string destination,
        CancellationToken cancellationToken = default);
}
