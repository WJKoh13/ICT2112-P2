using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Ecobadge
{
    private int _badgeid;
    public int Badgeid { get => _badgeid; set => _badgeid = value; }

    private double _maxcarbong;
    public double Maxcarbong { get => _maxcarbong; set => _maxcarbong = value; }

    private string? _criteriadescription;
    public string? Criteriadescription { get => _criteriadescription; set => _criteriadescription = value; }

    private string _badgename = null!;
    public string Badgename { get => _badgename; set => _badgename = value; }

    public virtual ICollection<Productfootprint> Productfootprints { get; private set; } = new List<Productfootprint>();
}
