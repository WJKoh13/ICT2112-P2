using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class RouteLeg
{
    private int _legId;
    public int LegId { get => _legId; set => _legId = value; }

    private int _routeId;
    public int RouteId { get => _routeId; set => _routeId = value; }

    private int? _sequence;
    public int? Sequence { get => _sequence; set => _sequence = value; }

    private string? _startPoint;
    public string? StartPoint { get => _startPoint; set => _startPoint = value; }

    private string? _endPoint;
    public string? EndPoint { get => _endPoint; set => _endPoint = value; }

    private double? _distanceKm;
    public double? DistanceKm { get => _distanceKm; set => _distanceKm = value; }

    private bool? _isFirstMile;
    public bool? IsFirstMile { get => _isFirstMile; set => _isFirstMile = value; }

    private bool? _isLastMile;
    public bool? IsLastMile { get => _isLastMile; set => _isLastMile = value; }

    private int? _transportId;
    public int? TransportId { get => _transportId; set => _transportId = value; }

    public virtual DeliveryRoute Route { get; private set; } = null!;

    public virtual Transport? Transport { get; private set; }
}
