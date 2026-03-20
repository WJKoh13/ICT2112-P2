using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Suppliercategorychangelog
{
    private int _logid;
    private int Logid { get => _logid; set => _logid = value; }

    private int? _supplierid;
    private int? Supplierid { get => _supplierid; set => _supplierid = value; }

    private string? _changereason;
    private string? Changereason { get => _changereason; set => _changereason = value; }

    private DateTime? _changedat;
    private DateTime? Changedat { get => _changedat; set => _changedat = value; }

    public virtual Supplier? Supplier { get; private set; }
}
