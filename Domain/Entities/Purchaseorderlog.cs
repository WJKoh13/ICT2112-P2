using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Purchaseorderlog
{
    private int _purchaseorderlogid;
    public int Purchaseorderlogid { get => _purchaseorderlogid; set => _purchaseorderlogid = value; }

    private int _poid;
    public int Poid { get => _poid; set => _poid = value; }

    private DateTime? _podate;
    public DateTime? Podate { get => _podate; set => _podate = value; }

    private int? _supplierid;
    public int? Supplierid { get => _supplierid; set => _supplierid = value; }

    private DateTime? _expecteddeliverydate;
    public DateTime? Expecteddeliverydate { get => _expecteddeliverydate; set => _expecteddeliverydate = value; }

    private decimal? _totalamount;
    public decimal? Totalamount { get => _totalamount; set => _totalamount = value; }

    private string? _detailsjson;
    public string? Detailsjson { get => _detailsjson; set => _detailsjson = value; }

    public virtual Purchaseorder Po { get; private set; } = null!;

    public virtual Transactionlog PurchaseorderlogNavigation { get; private set; } = null!;
}
