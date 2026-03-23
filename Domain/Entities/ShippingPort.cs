using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class ShippingPort
{
    private int _hubId;
    public int HubId { get => _hubId; set => _hubId = value; }

    private string _portCode = null!;
    public string PortCode { get => _portCode; set => _portCode = value; }

    private string _portName = null!;
    public string PortName { get => _portName; set => _portName = value; }

    private string? _portType;
    public string? PortType { get => _portType; set => _portType = value; }

    private int? _vesselSize;
    public int? VesselSize { get => _vesselSize; set => _vesselSize = value; }

    public virtual TransportationHub Hub { get; private set; } = null!;
}
