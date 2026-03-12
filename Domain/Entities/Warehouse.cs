using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Warehouse
{
    public int HubId { get; set; }

    public string WarehouseCode { get; set; } = null!;

    public double? TotalWarehouseVolume { get; set; }

    public double? ClimateControlEmissionRate { get; set; }

    public double? LightingEmissionRate { get; set; }

    public double? SecuritySystemEmissionRate { get; set; }

    public virtual TransportationHub Hub { get; set; } = null!;
}
