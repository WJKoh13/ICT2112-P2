using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Warehouse
{
    private int _hubId;
    public int HubId { get => _hubId; set => _hubId = value; }

    private string _warehouseCode = null!;
    public string WarehouseCode { get => _warehouseCode; set => _warehouseCode = value; }

    private int? _maxProductCapacity;
    public int? MaxProductCapacity { get => _maxProductCapacity; set => _maxProductCapacity = value; }

    private double? _totalWarehouseVolume;
    public double? TotalWarehouseVolume { get => _totalWarehouseVolume; set => _totalWarehouseVolume = value; }

    private double? _climateControlEmissionRate;
    public double? ClimateControlEmissionRate { get => _climateControlEmissionRate; set => _climateControlEmissionRate = value; }

    private double? _lightingEmissionRate;
    public double? LightingEmissionRate { get => _lightingEmissionRate; set => _lightingEmissionRate = value; }

    private double? _securitySystemEmissionRate;
    public double? SecuritySystemEmissionRate { get => _securitySystemEmissionRate; set => _securitySystemEmissionRate = value; }

    public virtual TransportationHub Hub { get; private set; } = null!;
}
