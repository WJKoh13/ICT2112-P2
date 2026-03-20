using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Rentalorderlog
{
    private int _rentalorderlogid;
    private int Rentalorderlogid { get => _rentalorderlogid; set => _rentalorderlogid = value; }

    private int? _orderid;
    private int? Orderid { get => _orderid; set => _orderid = value; }

    private int? _customerid;
    private int? Customerid { get => _customerid; set => _customerid = value; }

    private DateTime? _orderdate;
    private DateTime? Orderdate { get => _orderdate; set => _orderdate = value; }

    private decimal? _totalamount;
    private decimal? Totalamount { get => _totalamount; set => _totalamount = value; }

    private string? _detailsjson;
    private string? Detailsjson { get => _detailsjson; set => _detailsjson = value; }

    public virtual ICollection<Loanlog> Loanlogs { get; private set; } = new List<Loanlog>();

    public virtual Order? Order { get; private set; }

    public virtual Transactionlog RentalorderlogNavigation { get; private set; } = null!;

    public virtual ICollection<Returnlog> Returnlogs { get; private set; } = new List<Returnlog>();
}
