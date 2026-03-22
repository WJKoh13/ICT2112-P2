using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class TransportationHub
{
    private int _hubId;
    public int HubId { get => _hubId; set => _hubId = value; }

    private double _longitude;
    public double Longitude { get => _longitude; set => _longitude = value; }

    private double _latitude;
    public double Latitude { get => _latitude; set => _latitude = value; }

    private string _countryCode = null!;
    public string CountryCode { get => _countryCode; set => _countryCode = value; }

    private string _address = null!;
    public string Address { get => _address; set => _address = value; }

    private string? _operationalStatus;
    public string? OperationalStatus { get => _operationalStatus; set => _operationalStatus = value; }

    private string? _operationTime;
    public string? OperationTime { get => _operationTime; set => _operationTime = value; }

    public virtual Airport? Airport { get; private set; }

    public virtual ICollection<DeliveryBatch> DeliveryBatches { get; private set; } = new List<DeliveryBatch>();

    public virtual ICollection<DeliveryRoute> DeliveryRouteDestinationHubs { get; private set; } = new List<DeliveryRoute>();

    public virtual ICollection<DeliveryRoute> DeliveryRouteOriginHubs { get; private set; } = new List<DeliveryRoute>();

    public virtual ShippingPort? ShippingPort { get; private set; }

    public virtual Warehouse? Warehouse { get; private set; }
}
