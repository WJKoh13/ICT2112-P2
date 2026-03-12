using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Purchaseorder
{
    public int Poid { get; set; }

    public int? Supplierid { get; set; }

    public DateOnly? Podate { get; set; }

    public DateOnly? Expecteddeliverydate { get; set; }

    public decimal? Totalamount { get; set; }

    public virtual ICollection<Polineitem> Polineitems { get; set; } = new List<Polineitem>();

    public virtual ICollection<Purchaseorderlog> Purchaseorderlogs { get; set; } = new List<Purchaseorderlog>();
}
