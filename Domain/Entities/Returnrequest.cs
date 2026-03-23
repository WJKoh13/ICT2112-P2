using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Returnrequest
{
    private int _returnrequestid;
    public int Returnrequestid { get => _returnrequestid; set => _returnrequestid = value; }

    private int _orderid;
    public int Orderid { get => _orderid; set => _orderid = value; }

    private int _customerid;
    public int Customerid { get => _customerid; set => _customerid = value; }

    private DateTime _requestdate;
    public DateTime Requestdate { get => _requestdate; set => _requestdate = value; }

    private DateTime? _completiondate;
    public DateTime? Completiondate { get => _completiondate; set => _completiondate = value; }

    public virtual Customer Customer { get; private set; } = null!;

    public virtual Order Order { get; private set; } = null!;

    public virtual ICollection<Refund> Refunds { get; private set; } = new List<Refund>();

    public virtual ICollection<ReturnStage> ReturnStages { get; private set; } = new List<ReturnStage>();

    public virtual ICollection<Returnitem> Returnitems { get; private set; } = new List<Returnitem>();

    public virtual ICollection<Returnlog> Returnlogs { get; private set; } = new List<Returnlog>();
}
