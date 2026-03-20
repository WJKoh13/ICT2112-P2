using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Productfootprint
{
    private int _productcarbonfootprintid;
    private int Productcarbonfootprintid { get => _productcarbonfootprintid; set => _productcarbonfootprintid = value; }

    private int _productid;
    private int Productid { get => _productid; set => _productid = value; }

    private int _badgeid;
    private int Badgeid { get => _badgeid; set => _badgeid = value; }

    private double? _producttoxicpercentage;
    private double? Producttoxicpercentage { get => _producttoxicpercentage; set => _producttoxicpercentage = value; }

    private double _totalco2;
    private double Totalco2 { get => _totalco2; set => _totalco2 = value; }

    private DateTime _calculatedat;
    private DateTime Calculatedat { get => _calculatedat; set => _calculatedat = value; }

    public virtual Ecobadge Badge { get; private set; } = null!;

    public virtual Product Product { get; private set; } = null!;
}
