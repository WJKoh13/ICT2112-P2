using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Refund
{
    private int _refundid;
    private int Refundid { get => _refundid; set => _refundid = value; }

    private int _orderid;
    private int Orderid { get => _orderid; set => _orderid = value; }

    private int _customerid;
    private int Customerid { get => _customerid; set => _customerid = value; }

    private int? _transactionid;
    private int? Transactionid { get => _transactionid; set => _transactionid = value; }

    private int _returnrequestid;
    private int Returnrequestid { get => _returnrequestid; set => _returnrequestid = value; }

    private decimal _depositrefundamount;
    private decimal Depositrefundamount { get => _depositrefundamount; set => _depositrefundamount = value; }

    private DateTime _returndate;
    private DateTime Returndate { get => _returndate; set => _returndate = value; }

    private decimal? _penaltyamount;
    private decimal? Penaltyamount { get => _penaltyamount; set => _penaltyamount = value; }

    private string _returnmethod = null!;
    private string Returnmethod { get => _returnmethod; set => _returnmethod = value; }

    public virtual Customer Customer { get; private set; } = null!;

    public virtual Order Order { get; private set; } = null!;

    public virtual Returnrequest Returnrequest { get; private set; } = null!;

    public virtual Transaction? Transaction { get; private set; }
}
