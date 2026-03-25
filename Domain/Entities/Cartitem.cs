using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Cartitem
{
    private int _cartitemid;
    private int Cartitemid { get => _cartitemid; set => _cartitemid = value; }

    private int _cartid;
    private int Cartid { get => _cartid; set => _cartid = value; }

    private int _productid;
    private int Productid { get => _productid; set => _productid = value; }

    private int _quantity;
    private int Quantity { get => _quantity; set => _quantity = value; }

    private bool? _isselected;
    private bool? Isselected { get => _isselected; set => _isselected = value; }

    public virtual Cart Cart { get; private set; } = null!;

    public virtual Product Product { get; private set; } = null!;
}
