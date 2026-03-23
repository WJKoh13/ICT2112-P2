using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Loanlist
{
    private int _loanlistid;
    public int Loanlistid { get => _loanlistid; set => _loanlistid = value; }

    private int _orderid;
    public int Orderid { get => _orderid; set => _orderid = value; }

    private int _customerid;
    public int Customerid { get => _customerid; set => _customerid = value; }

    private DateTime _loandate;
    public DateTime Loandate { get => _loandate; set => _loandate = value; }

    private DateTime _duedate;
    public DateTime Duedate { get => _duedate; set => _duedate = value; }

    private DateTime? _returndate;
    public DateTime? Returndate { get => _returndate; set => _returndate = value; }

    private string? _remarks;
    public string? Remarks { get => _remarks; set => _remarks = value; }

    public virtual Customer Customer { get; private set; } = null!;

    public virtual ICollection<Loanitem> Loanitems { get; private set; } = new List<Loanitem>();

    public virtual ICollection<Loanlog> Loanlogs { get; private set; } = new List<Loanlog>();

    public virtual Order Order { get; private set; } = null!;
}
