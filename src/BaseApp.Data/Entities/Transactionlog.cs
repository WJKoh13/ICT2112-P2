using System;
using System.Collections.Generic;

namespace BaseApp.Data.Entities;

public partial class Transactionlog
{
    public int Transactionlogid { get; set; }

    public DateTime? Createdat { get; set; }

    public virtual Clearancelog? Clearancelog { get; set; }

    public virtual Loanlog? Loanlog { get; set; }

    public virtual Purchaseorderlog? Purchaseorderlog { get; set; }

    public virtual Rentalorderlog? Rentalorderlog { get; set; }

    public virtual Returnlog? Returnlog { get; set; }

    public virtual ICollection<Analytic> Analytics { get; set; } = new List<Analytic>();
}
