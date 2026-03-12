using System;
using System.Collections.Generic;

namespace BaseApp.Data.Entities;

public partial class Analytic
{
    public int Analyticsid { get; set; }

    public DateTime? Startdate { get; set; }

    public DateTime? Enddate { get; set; }

    public int? Loanamt { get; set; }

    public int? Returnamt { get; set; }

    public string? Primarysupplier { get; set; }

    public string? Primaryitem { get; set; }

    public decimal? Supplierreliability { get; set; }

    public decimal? Turnoverrate { get; set; }

    public virtual ICollection<Reportexport> Reportexports { get; set; } = new List<Reportexport>();

    public virtual ICollection<Transactionlog> Transactionlogs { get; set; } = new List<Transactionlog>();
}
