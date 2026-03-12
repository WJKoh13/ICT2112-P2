using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Stafffootprint
{
    public int Staffcarbonfootprintid { get; set; }

    public int Staffid { get; set; }

    public DateTime Time { get; set; }

    public double Hoursworked { get; set; }

    public double Totalstaffco2 { get; set; }

    public virtual Staff Staff { get; set; } = null!;
}
