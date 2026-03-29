namespace ProRental.Domain.Entities;

public partial class DeliveryRoute
{
    private int GetRouteId() => _routeId;
    private string GetOriginAddress() => _originAddress;
    private string GetDestinationAddress() => _destinationAddress;
    private double? GetTotalDistanceKm() => _totalDistanceKm;
    private bool? GetIsValid() => _isValid;
    private IReadOnlyList<RouteLeg> GetOrderedRouteLegs() => RouteLegs
        .OrderBy(routeLeg => routeLeg.ReadSequence() ?? int.MaxValue)
        .ToArray();

    private void SetRouteId(int routeId) => _routeId = routeId;
    private void SetOriginAddress(string originAddress) => _originAddress = originAddress;
    private void SetDestinationAddress(string destinationAddress) => _destinationAddress = destinationAddress;
    private void SetTotalDistanceKm(double totalDistanceKm) => _totalDistanceKm = totalDistanceKm;
    private void SetIsValid(bool isValid) => _isValid = isValid;

    public int ReadRouteId() => GetRouteId();
    public string ReadOriginAddress() => GetOriginAddress();
    public string ReadDestinationAddress() => GetDestinationAddress();
    public double? ReadTotalDistanceKm() => GetTotalDistanceKm();
    public bool? IsValidRoute() => GetIsValid();
    public IReadOnlyList<RouteLeg> ReadOrderedRouteLegs() => GetOrderedRouteLegs();

    public void addLeg(RouteLeg leg)
    {
        ArgumentNullException.ThrowIfNull(leg);
        RouteLegs.Add(leg);
    }

    public void InitializeRoute(string originAddress, string destinationAddress)
    {
        SetOriginAddress(originAddress);
        SetDestinationAddress(destinationAddress);
    }

    public void MarkAsValid() => SetIsValid(true);
    public void MarkAsInvalid() => SetIsValid(false);
    public void UpdateTotalDistanceKm(double totalDistanceKm) => SetTotalDistanceKm(totalDistanceKm);
}
