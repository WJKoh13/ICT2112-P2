using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Purchaseorderlog
{
    public int Purchaseorderlogid { get; set; }

    public int Poid { get; set; }

    public DateTime? Podate { get; set; }

    public int? Supplierid { get; set; }

    public DateTime? Expecteddeliverydate { get; set; }

    public decimal? Totalamount { get; set; }

    public string? Detailsjson { get; set; }

    public virtual Purchaseorder Po { get; set; } = null!;

    public virtual Transactionlog PurchaseorderlogNavigation { get; set; } = null!;
}
