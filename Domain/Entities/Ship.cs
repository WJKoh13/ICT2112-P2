using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Ship
{
    private int _transportId;
    public int TransportId { get => _transportId; set => _transportId = value; }

    private int _shipId;
    public int ShipId { get => _shipId; set => _shipId = value; }

    private string? _vesselType;
    public string? VesselType { get => _vesselType; set => _vesselType = value; }

    private string? _vesselNumber;
    public string? VesselNumber { get => _vesselNumber; set => _vesselNumber = value; }

    private string? _maxVesselSize;
    public string? MaxVesselSize { get => _maxVesselSize; set => _maxVesselSize = value; }

    public virtual Transport Transport { get; private set; } = null!;
}
