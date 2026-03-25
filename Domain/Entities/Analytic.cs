using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Analytic
{
    private int _analyticsid;
    private int Analyticsid { get => _analyticsid; set => _analyticsid = value; }

    private DateTime? _startdate;
    private DateTime? Startdate { get => _startdate; set => _startdate = value; }

    private DateTime? _enddate;
    private DateTime? Enddate { get => _enddate; set => _enddate = value; }

    private int? _loanamt;
    private int? Loanamt { get => _loanamt; set => _loanamt = value; }

    private int? _returnamt;
    private int? Returnamt { get => _returnamt; set => _returnamt = value; }

    private string? _primarysupplier;
    private string? Primarysupplier { get => _primarysupplier; set => _primarysupplier = value; }

    private string? _primaryitem;
    private string? Primaryitem { get => _primaryitem; set => _primaryitem = value; }

    private decimal? _supplierreliability;
    private decimal? Supplierreliability { get => _supplierreliability; set => _supplierreliability = value; }

    private decimal? _turnoverrate;
    private decimal? Turnoverrate { get => _turnoverrate; set => _turnoverrate = value; }

    public virtual ICollection<Reportexport> Reportexports { get; private set; } = new List<Reportexport>();

    public virtual ICollection<Transactionlog> Transactionlogs { get; private set; } = new List<Transactionlog>();
}
