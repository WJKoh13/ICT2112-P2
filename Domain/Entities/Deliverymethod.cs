using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Deliverymethod
{
    private int _deliveryid;
    private int Deliveryid { get => _deliveryid; set => _deliveryid = value; }

    private int _orderid;
    private int Orderid { get => _orderid; set => _orderid = value; }

    private int _durationdays;
    private int Durationdays { get => _durationdays; set => _durationdays = value; }

    private decimal _deliverycost;
    private decimal Deliverycost { get => _deliverycost; set => _deliverycost = value; }

    private string _carrierid = null!;
    private string Carrierid { get => _carrierid; set => _carrierid = value; }

    public virtual ICollection<Checkout> Checkouts { get; private set; } = new List<Checkout>();

    public virtual Order Order { get; private set; } = null!;
}
