using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class LegCarbon
{
    public int LegId { get; set; }

    public double? DistanceKm { get; set; }

    public double? WeightKg { get; set; }

    public double? CarbonKg { get; set; }

    public double? CarbonRate { get; set; }

    public int? CarbonResultId { get; set; }

    public int? RouteLegId { get; set; }

    public virtual CarbonResult? CarbonResult { get; set; }

    public virtual RouteLeg? RouteLeg { get; set; }
}
