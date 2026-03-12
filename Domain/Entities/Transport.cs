using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Transport
{
    public int TransportId { get; set; }

    public double? MaxLoadKg { get; set; }

    public double? VehicleSizeM2 { get; set; }

    public bool? IsAvailable { get; set; }

    public virtual Plane? Plane { get; set; }

    public virtual ICollection<RouteLeg> RouteLegs { get; set; } = new List<RouteLeg>();

    public virtual Ship? Ship { get; set; }

    public virtual Train? Train { get; set; }

    public virtual Truck? Truck { get; set; }
}
