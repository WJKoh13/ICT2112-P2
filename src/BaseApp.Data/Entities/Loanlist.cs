using System;
using System.Collections.Generic;

namespace BaseApp.Data.Entities;

public partial class Loanlist
{
    public int Loanlistid { get; set; }

    public int Orderid { get; set; }

    public int Customerid { get; set; }

    public DateTime Loandate { get; set; }

    public DateTime Duedate { get; set; }

    public DateTime? Returndate { get; set; }

    public string? Remarks { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<Loanitem> Loanitems { get; set; } = new List<Loanitem>();

    public virtual ICollection<Loanlog> Loanlogs { get; set; } = new List<Loanlog>();

    public virtual Order Order { get; set; } = null!;
}
