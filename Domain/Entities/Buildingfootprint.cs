using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Buildingfootprint
{
    private int _buildingcarbonfootprintid;
    private int Buildingcarbonfootprintid { get => _buildingcarbonfootprintid; set => _buildingcarbonfootprintid = value; }

    private DateTime _timehourly;
    private DateTime Timehourly { get => _timehourly; set => _timehourly = value; }

    private string? _zone;
    private string? Zone { get => _zone; set => _zone = value; }

    private string? _block;
    private string? Block { get => _block; set => _block = value; }

    private string? _floor;
    private string? Floor { get => _floor; set => _floor = value; }

    private string? _room;
    private string? Room { get => _room; set => _room = value; }

    private double _totalroomco2;
    private double Totalroomco2 { get => _totalroomco2; set => _totalroomco2 = value; }
}
