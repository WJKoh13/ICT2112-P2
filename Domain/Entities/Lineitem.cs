using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Lineitem
{
    public int Lineitemid { get; set; }

    public int? Requestid { get; set; }

    public int? Productid { get; set; }

    public int? Quantityrequest { get; set; }

    public string? Remarks { get; set; }

    public virtual Product? Product { get; set; }

    public virtual Replenishmentrequest? Request { get; set; }
}
