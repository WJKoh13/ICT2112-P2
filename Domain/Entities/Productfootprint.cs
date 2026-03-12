using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Productfootprint
{
    public int Productcarbonfootprintid { get; set; }

    public int Productid { get; set; }

    public int Badgeid { get; set; }

    public double? Producttoxicpercentage { get; set; }

    public double Totalco2 { get; set; }

    public DateTime Calculatedat { get; set; }

    public virtual Ecobadge Badge { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
