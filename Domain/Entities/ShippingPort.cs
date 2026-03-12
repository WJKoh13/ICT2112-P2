using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class ShippingPort
{
    public int HubId { get; set; }

    public string PortCode { get; set; } = null!;

    public string PortName { get; set; } = null!;

    public string? PortType { get; set; }

    public string? VesselSize { get; set; }

    public virtual TransportationHub Hub { get; set; } = null!;
}
