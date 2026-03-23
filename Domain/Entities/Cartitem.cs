using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Cartitem
{
    private int _cartitemid;
    public int Cartitemid { get => _cartitemid; set => _cartitemid = value; }

    private int _cartid;
    public int Cartid { get => _cartid; set => _cartid = value; }

    private int _productid;
    public int Productid { get => _productid; set => _productid = value; }

    private int _quantity;
    public int Quantity { get => _quantity; set => _quantity = value; }

    private bool? _isselected;
    public bool? Isselected { get => _isselected; set => _isselected = value; }

    public virtual Cart Cart { get; private set; } = null!;

    public virtual Product Product { get; private set; } = null!;
}
