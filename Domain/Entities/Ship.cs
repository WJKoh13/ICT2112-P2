using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Ship
{
    public int TransportId { get; set; }

    public int ShipId { get; set; }

    public string? VesselType { get; set; }

    public string? VesselNumber { get; set; }

    public string? MaxVesselSize { get; set; }

    public virtual Transport Transport { get; set; } = null!;
}
