using System;
using System.Collections.Generic;

namespace BaseApp.Data.Entities;

public partial class RouteLeg
{
    public int LegId { get; set; }

    public int RouteId { get; set; }

    public int? Sequence { get; set; }

    public string? StartPoint { get; set; }

    public string? EndPoint { get; set; }

    public double? DistanceKm { get; set; }

    public bool? IsFirstMile { get; set; }

    public bool? IsLastMile { get; set; }

    public int? TransportId { get; set; }

    public virtual ICollection<LegCarbon> LegCarbons { get; set; } = new List<LegCarbon>();

    public virtual Route Route { get; set; } = null!;

    public virtual Transport? Transport { get; set; }
}
