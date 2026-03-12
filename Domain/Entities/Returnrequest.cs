using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Returnrequest
{
    public int Returnrequestid { get; set; }

    public int Orderid { get; set; }

    public int Customerid { get; set; }

    public DateTime Requestdate { get; set; }

    public DateTime? Completiondate { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;

    public virtual ICollection<Returnitem> Returnitems { get; set; } = new List<Returnitem>();

    public virtual ICollection<Returnlog> Returnlogs { get; set; } = new List<Returnlog>();
}
