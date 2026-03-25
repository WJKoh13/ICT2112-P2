using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Purchaseorderlog
{
    private int _purchaseorderlogid;
    private int Purchaseorderlogid { get => _purchaseorderlogid; set => _purchaseorderlogid = value; }

    private int _poid;
    private int Poid { get => _poid; set => _poid = value; }

    private DateTime? _podate;
    private DateTime? Podate { get => _podate; set => _podate = value; }

    private int? _supplierid;
    private int? Supplierid { get => _supplierid; set => _supplierid = value; }

    private DateTime? _expecteddeliverydate;
    private DateTime? Expecteddeliverydate { get => _expecteddeliverydate; set => _expecteddeliverydate = value; }

    private decimal? _totalamount;
    private decimal? Totalamount { get => _totalamount; set => _totalamount = value; }

    private string? _detailsjson;
    private string? Detailsjson { get => _detailsjson; set => _detailsjson = value; }

    public virtual Purchaseorder Po { get; private set; } = null!;

    public virtual Transactionlog PurchaseorderlogNavigation { get; private set; } = null!;
}
