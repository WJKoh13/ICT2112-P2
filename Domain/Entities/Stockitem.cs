using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Stockitem
{
    public int Productid { get; set; }

    public string? Sku { get; set; }

    public string? Name { get; set; }

    public string? Uom { get; set; }

    public virtual ICollection<Polineitem> Polineitems { get; set; } = new List<Polineitem>();
}
