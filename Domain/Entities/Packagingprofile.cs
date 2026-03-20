using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Packagingprofile
{
    private int _profileid;
    private int Profileid { get => _profileid; set => _profileid = value; }

    private int _orderid;
    private int Orderid { get => _orderid; set => _orderid = value; }

    private double _volume;
    private double Volume { get => _volume; set => _volume = value; }

    private string? _fragilitylevel;
    private string? Fragilitylevel { get => _fragilitylevel; set => _fragilitylevel = value; }

    public virtual Order Order { get; private set; } = null!;

    public virtual ICollection<Packagingconfiguration> Packagingconfigurations { get; private set; } = new List<Packagingconfiguration>();
}
