using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Customerreward
{
    public int Customerrewardsid { get; set; }

    public int Customerid { get; set; }

    public double Discount { get; set; }

    public double Totalcarbon { get; set; }

    public virtual Customer Customer { get; set; } = null!;
}
