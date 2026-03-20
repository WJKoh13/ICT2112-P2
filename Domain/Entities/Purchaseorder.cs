using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Purchaseorder
{
    private int _poid;
    private int Poid { get => _poid; set => _poid = value; }

    private int? _supplierid;
    private int? Supplierid { get => _supplierid; set => _supplierid = value; }

    private DateOnly? _podate;
    private DateOnly? Podate { get => _podate; set => _podate = value; }

    private DateOnly? _expecteddeliverydate;
    private DateOnly? Expecteddeliverydate { get => _expecteddeliverydate; set => _expecteddeliverydate = value; }

    private decimal? _totalamount;
    private decimal? Totalamount { get => _totalamount; set => _totalamount = value; }

    public virtual ICollection<Polineitem> Polineitems { get; private set; } = new List<Polineitem>();

    public virtual ICollection<Purchaseorderlog> Purchaseorderlogs { get; private set; } = new List<Purchaseorderlog>();
}
