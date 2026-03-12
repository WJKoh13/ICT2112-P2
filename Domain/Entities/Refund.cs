using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Refund
{
    public int Refundid { get; set; }

    public int Orderid { get; set; }

    public int Customerid { get; set; }

    public decimal Depositrefundamount { get; set; }

    public DateTime Returndate { get; set; }

    public decimal? Penaltyamount { get; set; }

    public string Returnmethod { get; set; } = null!;

    public virtual Customer Customer { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;
}
