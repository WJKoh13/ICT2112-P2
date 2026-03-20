using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Cart
{
    private int _cartid;
    private int Cartid { get => _cartid; set => _cartid = value; }

    private int? _customerid;
    private int? Customerid { get => _customerid; set => _customerid = value; }

    private int? _sessionid;
    private int? Sessionid { get => _sessionid; set => _sessionid = value; }

    private DateTime? _rentalstart;
    private DateTime? Rentalstart { get => _rentalstart; set => _rentalstart = value; }

    private DateTime? _rentalend;
    private DateTime? Rentalend { get => _rentalend; set => _rentalend = value; }

    public virtual ICollection<Cartitem> Cartitems { get; private set; } = new List<Cartitem>();

    public virtual ICollection<Checkout> Checkouts { get; private set; } = new List<Checkout>();

    public virtual Customer? Customer { get; private set; }

    public virtual Session? Session { get; private set; }
}
