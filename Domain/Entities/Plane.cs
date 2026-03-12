using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Plane
{
    public int TransportId { get; set; }

    public int PlaneId { get; set; }

    public string? PlaneType { get; set; }

    public string? PlaneCallsign { get; set; }

    public virtual Transport Transport { get; set; } = null!;
}
