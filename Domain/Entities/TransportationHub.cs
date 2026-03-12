using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class TransportationHub
{
    public int HubId { get; set; }

    public double Longitude { get; set; }

    public double Latitude { get; set; }

    public string CountryCode { get; set; } = null!;

    public string Address { get; set; } = null!;

    public string? OperationalStatus { get; set; }

    public string? OperationTime { get; set; }

    public virtual Airport? Airport { get; set; }

    public virtual ICollection<DeliveryBatch> DeliveryBatches { get; set; } = new List<DeliveryBatch>();

    public virtual ICollection<Route> RouteDestinationHubs { get; set; } = new List<Route>();

    public virtual ICollection<Route> RouteOriginHubs { get; set; } = new List<Route>();

    public virtual ShippingPort? ShippingPort { get; set; }

    public virtual Warehouse? Warehouse { get; set; }
}
