using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class DeliveryRoute
{
    private int _routeId;
    public int RouteId { get => _routeId; set => _routeId = value; }

    private string _originAddress = null!;
    public string OriginAddress { get => _originAddress; set => _originAddress = value; }

    private string _destinationAddress = null!;
    public string DestinationAddress { get => _destinationAddress; set => _destinationAddress = value; }

    private double? _totalDistanceKm;
    public double? TotalDistanceKm { get => _totalDistanceKm; set => _totalDistanceKm = value; }

    private bool? _isValid;
    public bool? IsValid { get => _isValid; set => _isValid = value; }

    private int? _originHubId;
    public int? OriginHubId { get => _originHubId; set => _originHubId = value; }

    private int? _destinationHubId;
    public int? DestinationHubId { get => _destinationHubId; set => _destinationHubId = value; }

    public virtual TransportationHub? DestinationHub { get; private set; }

    public virtual TransportationHub? OriginHub { get; private set; }

    public virtual ICollection<RouteLeg> RouteLegs { get; private set; } = new List<RouteLeg>();

    public virtual ICollection<ShippingOption> ShippingOptions { get; private set; } = new List<ShippingOption>();
}
