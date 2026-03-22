using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Truck
{
    private int _transportId;
    public int TransportId { get => _transportId; set => _transportId = value; }

    private int _truckId;
    public int TruckId { get => _truckId; set => _truckId = value; }

    private string? _truckType;
    public string? TruckType { get => _truckType; set => _truckType = value; }

    private string? _licensePlate;
    public string? LicensePlate { get => _licensePlate; set => _licensePlate = value; }

    public virtual Transport Transport { get; private set; } = null!;
}
