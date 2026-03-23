using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Rentalorderlog
{
    private int _rentalorderlogid;
    public int Rentalorderlogid { get => _rentalorderlogid; set => _rentalorderlogid = value; }

    private int? _orderid;
    public int? Orderid { get => _orderid; set => _orderid = value; }

    private int? _customerid;
    public int? Customerid { get => _customerid; set => _customerid = value; }

    private DateTime? _orderdate;
    public DateTime? Orderdate { get => _orderdate; set => _orderdate = value; }

    private decimal? _totalamount;
    public decimal? Totalamount { get => _totalamount; set => _totalamount = value; }

    private string? _detailsjson;
    public string? Detailsjson { get => _detailsjson; set => _detailsjson = value; }

    public virtual ICollection<Loanlog> Loanlogs { get; private set; } = new List<Loanlog>();

    public virtual Order? Order { get; private set; }

    public virtual Transactionlog RentalorderlogNavigation { get; private set; } = null!;

    public virtual ICollection<Returnlog> Returnlogs { get; private set; } = new List<Returnlog>();
}
