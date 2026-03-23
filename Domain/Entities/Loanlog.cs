using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Loanlog
{
    private int _loanlogid;
    public int Loanlogid { get => _loanlogid; set => _loanlogid = value; }

    private int _loanlistid;
    public int Loanlistid { get => _loanlistid; set => _loanlistid = value; }

    private int _rentalorderlogid;
    public int Rentalorderlogid { get => _rentalorderlogid; set => _rentalorderlogid = value; }

    private DateTime? _loandate;
    public DateTime? Loandate { get => _loandate; set => _loandate = value; }

    private DateTime? _returndate;
    public DateTime? Returndate { get => _returndate; set => _returndate = value; }

    private DateTime? _duedate;
    public DateTime? Duedate { get => _duedate; set => _duedate = value; }

    private string? _detailsjson;
    public string? Detailsjson { get => _detailsjson; set => _detailsjson = value; }

    public virtual Loanlist Loanlist { get; private set; } = null!;

    public virtual Transactionlog LoanlogNavigation { get; private set; } = null!;

    public virtual Rentalorderlog Rentalorderlog { get; private set; } = null!;
}
