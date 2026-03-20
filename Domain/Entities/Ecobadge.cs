using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Ecobadge
{
    private int _badgeid;
    private int Badgeid { get => _badgeid; set => _badgeid = value; }

    private double _maxcarbong;
    private double Maxcarbong { get => _maxcarbong; set => _maxcarbong = value; }

    private string? _criteriadescription;
    private string? Criteriadescription { get => _criteriadescription; set => _criteriadescription = value; }

    private string _badgename = null!;
    private string Badgename { get => _badgename; set => _badgename = value; }

    public virtual ICollection<Productfootprint> Productfootprints { get; private set; } = new List<Productfootprint>();
}
