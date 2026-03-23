using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Clearancelog
{
    private int _clearancelogid;
    public int Clearancelogid { get => _clearancelogid; set => _clearancelogid = value; }

    private int _clearancebatchid;
    public int Clearancebatchid { get => _clearancebatchid; set => _clearancebatchid = value; }

    private string? _batchname;
    public string? Batchname { get => _batchname; set => _batchname = value; }

    private DateTime? _clearancedate;
    public DateTime? Clearancedate { get => _clearancedate; set => _clearancedate = value; }

    private string? _detailsjson;
    public string? Detailsjson { get => _detailsjson; set => _detailsjson = value; }

    public virtual Clearancebatch Clearancebatch { get; private set; } = null!;

    public virtual Transactionlog ClearancelogNavigation { get; private set; } = null!;
}
