using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Transactionlog
{
    private int _transactionlogid;
    private int Transactionlogid { get => _transactionlogid; set => _transactionlogid = value; }

    private DateTime? _createdat;
    private DateTime? Createdat { get => _createdat; set => _createdat = value; }

    public virtual Clearancelog? Clearancelog { get; private set; }

    public virtual Loanlog? Loanlog { get; private set; }

    public virtual Purchaseorderlog? Purchaseorderlog { get; private set; }

    public virtual Rentalorderlog? Rentalorderlog { get; private set; }

    public virtual Returnlog? Returnlog { get; private set; }

    public virtual ICollection<Analytic> Analytics { get; private set; } = new List<Analytic>();
}
