using System;
using System.Collections.Generic;

namespace BaseApp.Data.Entities;

public partial class Loanlog
{
    public int Loanlogid { get; set; }

    public int Loanlistid { get; set; }

    public int Rentalorderlogid { get; set; }

    public DateTime? Loandate { get; set; }

    public DateTime? Returndate { get; set; }

    public DateTime? Duedate { get; set; }

    public string? Detailsjson { get; set; }

    public virtual Loanlist Loanlist { get; set; } = null!;

    public virtual Transactionlog LoanlogNavigation { get; set; } = null!;

    public virtual Rentalorderlog Rentalorderlog { get; set; } = null!;
}
