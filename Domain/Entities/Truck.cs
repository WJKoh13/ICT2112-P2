using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Truck
{
    public int TransportId { get; set; }

    public int TruckId { get; set; }

    public string? TruckType { get; set; }

    public string? LicensePlate { get; set; }

    public virtual Transport Transport { get; set; } = null!;
}
