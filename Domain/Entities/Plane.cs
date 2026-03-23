using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Plane
{
    private int _transportId;
    public int TransportId { get => _transportId; set => _transportId = value; }

    private int _planeId;
    public int PlaneId { get => _planeId; set => _planeId = value; }

    private string? _planeType;
    public string? PlaneType { get => _planeType; set => _planeType = value; }

    private string? _planeCallsign;
    public string? PlaneCallsign { get => _planeCallsign; set => _planeCallsign = value; }

    public virtual Transport Transport { get; private set; } = null!;
}
