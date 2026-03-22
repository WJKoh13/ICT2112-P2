using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Stockitem
{
    private int _productid;
    public int Productid { get => _productid; set => _productid = value; }

    private string? _sku;
    public string? Sku { get => _sku; set => _sku = value; }

    private string? _name;
    public string? Name { get => _name; set => _name = value; }

    private string? _uom;
    public string? Uom { get => _uom; set => _uom = value; }

    public virtual ICollection<Polineitem> Polineitems { get; private set; } = new List<Polineitem>();
}
