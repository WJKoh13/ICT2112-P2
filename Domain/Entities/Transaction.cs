using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Transaction
{
    private int _transactionid;
    private int Transactionid { get => _transactionid; set => _transactionid = value; }

    private decimal _amount;
    private decimal Amount { get => _amount; set => _amount = value; }

    private string? _providertransactionid;
    private string? Providertransactionid { get => _providertransactionid; set => _providertransactionid = value; }

    private DateTime _createdat;
    private DateTime Createdat { get => _createdat; set => _createdat = value; }

    public virtual ICollection<Deposit> Deposits { get; private set; } = new List<Deposit>();

    public virtual ICollection<Order> Orders { get; private set; } = new List<Order>();

    public virtual ICollection<Payment> Payments { get; private set; } = new List<Payment>();

    public virtual ICollection<Refund> Refunds { get; private set; } = new List<Refund>();
}
