using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Checkout
{
    private int _checkoutid;
    public int Checkoutid { get => _checkoutid; set => _checkoutid = value; }

    private int _customerid;
    public int Customerid { get => _customerid; set => _customerid = value; }

    private int _cartid;
    public int Cartid { get => _cartid; set => _cartid = value; }

    private int? _optionId;
    public int? OptionId { get => _optionId; set => _optionId = value; }

    private bool? _notifyoptin;
    public bool? Notifyoptin { get => _notifyoptin; set => _notifyoptin = value; }

    private DateTime _createdat;
    public DateTime Createdat { get => _createdat; set => _createdat = value; }

    public virtual Cart Cart { get; private set; } = null!;

    public virtual Customer Customer { get; private set; } = null!;

    public virtual ShippingOption? Option { get; private set; }

    public virtual ICollection<Order> Orders { get; private set; } = new List<Order>();
}
