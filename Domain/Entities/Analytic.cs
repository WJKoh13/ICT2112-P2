using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Analytic
{
    public int Analyticsid { get; private set; }

    public DateTime? Startdate { get; private set; }

    public DateTime? Enddate { get; private set; }

    public int? Loanamt { get; private set; }

    public int? Returnamt { get; private set; }

    public int? Refprimaryid { get; private set; }

    public string? Refprimaryname { get; private set; }

    public decimal? Refvalue { get; private set; }

    public virtual ICollection<Reportexport> Reportexports { get; private set; } = new List<Reportexport>();

    public virtual ICollection<Transactionlog> Transactionlogs { get; private set; } = new List<Transactionlog>();
}
