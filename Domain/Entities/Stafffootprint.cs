using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Stafffootprint
{
    private int _staffcarbonfootprintid;
    public int Staffcarbonfootprintid { get => _staffcarbonfootprintid; set => _staffcarbonfootprintid = value; }

    private int _staffid;
    public int Staffid { get => _staffid; set => _staffid = value; }

    private DateTime _time;
    public DateTime Time { get => _time; set => _time = value; }

    private double _hoursworked;
    public double Hoursworked { get => _hoursworked; set => _hoursworked = value; }

    private double _totalstaffco2;
    public double Totalstaffco2 { get => _totalstaffco2; set => _totalstaffco2 = value; }

    public virtual Staff Staff { get; private set; } = null!;
}
