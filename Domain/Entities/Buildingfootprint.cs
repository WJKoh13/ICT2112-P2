using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Buildingfootprint
{
    public int Buildingcarbonfootprintid { get; set; }

    public DateTime Timehourly { get; set; }

    public string? Zone { get; set; }

    public string? Block { get; set; }

    public string? Floor { get; set; }

    public string? Room { get; set; }

    public double Totalroomco2 { get; set; }
}
