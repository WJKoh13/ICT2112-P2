using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Productfootprint
{
    private int _productcarbonfootprintid;
    public int Productcarbonfootprintid { get => _productcarbonfootprintid; set => _productcarbonfootprintid = value; }

    private int _productid;
    public int Productid { get => _productid; set => _productid = value; }

    private int _badgeid;
    public int Badgeid { get => _badgeid; set => _badgeid = value; }

    private double? _producttoxicpercentage;
    public double? Producttoxicpercentage { get => _producttoxicpercentage; set => _producttoxicpercentage = value; }

    private double _totalco2;
    public double Totalco2 { get => _totalco2; set => _totalco2 = value; }

    private DateTime _calculatedat;
    public DateTime Calculatedat { get => _calculatedat; set => _calculatedat = value; }

    public virtual Ecobadge Badge { get; private set; } = null!;

    public virtual Product Product { get; private set; } = null!;
}
