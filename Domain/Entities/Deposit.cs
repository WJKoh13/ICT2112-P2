using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Deposit
{
    private string _depositid = null!;
    public string Depositid { get => _depositid; set => _depositid = value; }

    private int _orderid;
    public int Orderid { get => _orderid; set => _orderid = value; }

    private int _transactionid;
    public int Transactionid { get => _transactionid; set => _transactionid = value; }

    private decimal _originalamount;
    public decimal Originalamount { get => _originalamount; set => _originalamount = value; }

    private decimal _heldamount;
    public decimal Heldamount { get => _heldamount; set => _heldamount = value; }

    private decimal? _refundedamount;
    public decimal? Refundedamount { get => _refundedamount; set => _refundedamount = value; }

    private decimal? _forfeitedamount;
    public decimal? Forfeitedamount { get => _forfeitedamount; set => _forfeitedamount = value; }

    private DateTime _createdat;
    public DateTime Createdat { get => _createdat; set => _createdat = value; }

    public virtual Order Order { get; private set; } = null!;

    public virtual Transaction Transaction { get; private set; } = null!;
}
