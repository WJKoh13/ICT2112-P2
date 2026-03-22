using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Orderstatushistory
{
    private int _historyid;
    public int Historyid { get => _historyid; set => _historyid = value; }

    private int _orderid;
    public int Orderid { get => _orderid; set => _orderid = value; }

    private DateTime _timestamp;
    public DateTime Timestamp { get => _timestamp; set => _timestamp = value; }

    private string _updatedby = null!;
    public string Updatedby { get => _updatedby; set => _updatedby = value; }

    private string? _remark;
    public string? Remark { get => _remark; set => _remark = value; }

    public virtual Order Order { get; private set; } = null!;
}
