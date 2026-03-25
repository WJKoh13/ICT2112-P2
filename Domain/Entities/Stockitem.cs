using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Stockitem
{
    private int _productid;
    private int Productid { get => _productid; set => _productid = value; }

    private string? _sku;
    private string? Sku { get => _sku; set => _sku = value; }

    private string? _name;
    private string? Name { get => _name; set => _name = value; }

    private string? _uom;
    private string? Uom { get => _uom; set => _uom = value; }

    public virtual ICollection<Polineitem> Polineitems { get; private set; } = new List<Polineitem>();
}
