using ProRental.Domain.Enums;

namespace ProRental.Domain.Entities;

public partial class RouteLeg
{
    private TransportMode? _transportMode;
    private TransportMode? TransportMode { get => _transportMode; set => _transportMode = value; }

    private int GetLegId() => _legId;
    private int GetRouteId() => _routeId;
    private int? GetSequence() => _sequence;
    private string GetStartPoint() => _startPoint ?? string.Empty;
    private string GetEndPoint() => _endPoint ?? string.Empty;
    private double? GetDistanceKm() => _distanceKm;
    private bool? GetIsFirstMile() => _isFirstMile;
    private bool? GetIsMainTransport() => _isMainTransport;
    private bool? GetIsLastMile() => _isLastMile;
    private TransportMode? GetTransportMode() => _transportMode;

    private void SetLegId(int legId) => _legId = legId;
    private void SetRouteId(int routeId) => _routeId = routeId;
    private void SetSequence(int sequence) => _sequence = sequence;
    private void SetStartPoint(string startPoint) => _startPoint = startPoint;
    private void SetEndPoint(string endPoint) => _endPoint = endPoint;
    private void SetDistanceKm(double distanceKm) => _distanceKm = distanceKm;
    private void SetIsFirstMile(bool isFirstMile) => _isFirstMile = isFirstMile;
    private void SetIsMainTransport(bool isMainTransport) => _isMainTransport = isMainTransport;
    private void SetIsLastMile(bool isLastMile) => _isLastMile = isLastMile;
    private void SetTransportMode(TransportMode transportMode) => _transportMode = transportMode;

    public int ReadLegId() => GetLegId();
    public int ReadRouteId() => GetRouteId();
    public int? ReadSequence() => GetSequence();
    public string ReadStartPoint() => GetStartPoint();
    public string ReadEndPoint() => GetEndPoint();
    public double? ReadDistanceKm() => GetDistanceKm();
    public bool? ReadIsFirstMile() => GetIsFirstMile();
    public bool? ReadIsMainTransport() => GetIsMainTransport();
    public bool? ReadIsLastMile() => GetIsLastMile();
    public TransportMode? ReadTransportMode() => GetTransportMode();

    public void UpdateTransportMode(TransportMode newValue) => _transportMode = newValue;

    public void ConfigureLeg(
        int sequence,
        string startPoint,
        string endPoint,
        double distanceKm,
        TransportMode transportMode,
        bool isFirstMile,
        bool isMainTransport,
        bool isLastMile)
    {
        var isSingleLeg = isFirstMile && isLastMile;
        var computedIsMainTransport = isMainTransport || isSingleLeg || (!isFirstMile && !isLastMile);

        SetSequence(sequence);
        SetStartPoint(startPoint);
        SetEndPoint(endPoint);
        SetDistanceKm(distanceKm);
        SetTransportMode(transportMode);
        SetIsFirstMile(isFirstMile && !isSingleLeg);
        SetIsMainTransport(computedIsMainTransport);
        SetIsLastMile(isLastMile && !isSingleLeg);
    }

    public void ConfigureLeg(
        int sequence,
        string startPoint,
        string endPoint,
        double distanceKm,
        TransportMode transportMode,
        bool isFirstMile,
        bool isLastMile)
    {
        ConfigureLeg(
            sequence,
            startPoint,
            endPoint,
            distanceKm,
            transportMode,
            isFirstMile,
            isMainTransport: false,
            isLastMile);
    }
}
