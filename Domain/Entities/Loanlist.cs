using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Loanlist
{
    private int _loanlistid;
    private int Loanlistid { get => _loanlistid; set => _loanlistid = value; }

    private int _orderid;
    private int Orderid { get => _orderid; set => _orderid = value; }

    private int _customerid;
    private int Customerid { get => _customerid; set => _customerid = value; }

    private DateTime _loandate;
    private DateTime Loandate { get => _loandate; set => _loandate = value; }

    private DateTime _duedate;
    private DateTime Duedate { get => _duedate; set => _duedate = value; }

    private DateTime? _returndate;
    private DateTime? Returndate { get => _returndate; set => _returndate = value; }

    private string? _remarks;
    private string? Remarks { get => _remarks; set => _remarks = value; }

    public virtual Customer Customer { get; private set; } = null!;

    public virtual ICollection<Loanitem> Loanitems { get; private set; } = new List<Loanitem>();

    public virtual ICollection<Loanlog> Loanlogs { get; private set; } = new List<Loanlog>();

    public virtual Order Order { get; private set; } = null!;
}
