namespace ProRental.Domain.Entities;

/// <summary>
/// Feature 1 accessors for the shared DeliveryRoute entity. These helpers let checkout
/// consume route identity and distance state without opening the full EF model publicly.
/// by: ernest
/// </summary>
public partial class DeliveryRoute
{
    private int? _feature1OriginHubId;
    private int? _feature1DestinationHubId;

    public int GetRouteId() => _routeId;
    public string GetOriginAddress() => _originAddress;
    public string GetDestinationAddress() => _destinationAddress;
    public double GetTotalDistanceKm() => _totalDistanceKm;
    public bool GetIsValid() => _isValid;
    public int? GetOriginHubId() => _feature1OriginHubId;
    public int? GetDestinationHubId() => _feature1DestinationHubId;
    public IReadOnlyList<RouteLeg> GetOrderedRouteLegs() => RouteLegs
        .OrderBy(routeLeg => routeLeg.GetSequence())
        .ToArray();

    public void SetOriginAddress(string originAddress) => _originAddress = originAddress;
    public void SetDestinationAddress(string destinationAddress) => _destinationAddress = destinationAddress;
    public void SetTotalDistanceKm(double totalDistanceKm) => _totalDistanceKm = totalDistanceKm;
    public void SetIsValid(bool isValid) => _isValid = isValid;
    public void SetOriginHubId(int? originHubId) => _feature1OriginHubId = originHubId;
    public void SetDestinationHubId(int? destinationHubId) => _feature1DestinationHubId = destinationHubId;
}
