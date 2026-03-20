using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Deposit
{
    private string _depositid = null!;
    private string Depositid { get => _depositid; set => _depositid = value; }

    private int _orderid;
    private int Orderid { get => _orderid; set => _orderid = value; }

    private int _transactionid;
    private int Transactionid { get => _transactionid; set => _transactionid = value; }

    private decimal _originalamount;
    private decimal Originalamount { get => _originalamount; set => _originalamount = value; }

    private decimal _heldamount;
    private decimal Heldamount { get => _heldamount; set => _heldamount = value; }

    private decimal? _refundedamount;
    private decimal? Refundedamount { get => _refundedamount; set => _refundedamount = value; }

    private decimal? _forfeitedamount;
    private decimal? Forfeitedamount { get => _forfeitedamount; set => _forfeitedamount = value; }

    private DateTime _createdat;
    private DateTime Createdat { get => _createdat; set => _createdat = value; }

    public virtual Order Order { get; private set; } = null!;

    public virtual Transaction Transaction { get; private set; } = null!;
}
