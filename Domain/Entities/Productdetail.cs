using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Productdetail
{
    public int Detailsid { get; set; }

    public int Productid { get; set; }

    public int Totalquantity { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal? Weight { get; set; }

    public string? Image { get; set; }

    public decimal Price { get; set; }

    public decimal? Depositrate { get; set; }

    public virtual Product Product { get; set; } = null!;
}
