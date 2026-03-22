using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Refund
{
    private int _refundid;
    public int Refundid { get => _refundid; set => _refundid = value; }

    private int _orderid;
    public int Orderid { get => _orderid; set => _orderid = value; }

    private int _customerid;
    public int Customerid { get => _customerid; set => _customerid = value; }

    private int? _transactionid;
    public int? Transactionid { get => _transactionid; set => _transactionid = value; }

    private int _returnrequestid;
    public int Returnrequestid { get => _returnrequestid; set => _returnrequestid = value; }

    private decimal _depositrefundamount;
    public decimal Depositrefundamount { get => _depositrefundamount; set => _depositrefundamount = value; }

    private DateTime _returndate;
    public DateTime Returndate { get => _returndate; set => _returndate = value; }

    private decimal? _penaltyamount;
    public decimal? Penaltyamount { get => _penaltyamount; set => _penaltyamount = value; }

    private string _returnmethod = null!;
    public string Returnmethod { get => _returnmethod; set => _returnmethod = value; }

    public virtual Customer Customer { get; private set; } = null!;

    public virtual Order Order { get; private set; } = null!;

    public virtual Returnrequest Returnrequest { get; private set; } = null!;

    public virtual Transaction? Transaction { get; private set; }
}
