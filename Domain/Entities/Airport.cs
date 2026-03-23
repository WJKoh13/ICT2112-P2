using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Airport
{
    private int _hubId;
    public int HubId { get => _hubId; set => _hubId = value; }

    private string _airportCode = null!;
    public string AirportCode { get => _airportCode; set => _airportCode = value; }

    private string _airportName = null!;
    public string AirportName { get => _airportName; set => _airportName = value; }

    private int? _terminal;
    public int? Terminal { get => _terminal; set => _terminal = value; }

    private int? _aircraftSize;
    public int? AircraftSize { get => _aircraftSize; set => _aircraftSize = value; }

    public virtual TransportationHub Hub { get; private set; } = null!;
}
