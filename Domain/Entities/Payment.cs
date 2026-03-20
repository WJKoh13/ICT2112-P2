using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Payment
{
    private string _paymentid = null!;
    private string Paymentid { get => _paymentid; set => _paymentid = value; }

    private int _orderid;
    private int Orderid { get => _orderid; set => _orderid = value; }

    private int _transactionid;
    private int Transactionid { get => _transactionid; set => _transactionid = value; }

    private decimal _amount;
    private decimal Amount { get => _amount; set => _amount = value; }

    private DateTime _createdat;
    private DateTime Createdat { get => _createdat; set => _createdat = value; }

    public virtual Order Order { get; private set; } = null!;

    public virtual Transaction Transaction { get; private set; } = null!;
}
