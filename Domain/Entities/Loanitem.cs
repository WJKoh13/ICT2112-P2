using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Loanitem
{
    private int _loanitemid;
    private int Loanitemid { get => _loanitemid; set => _loanitemid = value; }

    private int _loanlistid;
    private int Loanlistid { get => _loanlistid; set => _loanlistid = value; }

    private int _inventoryitemid;
    private int Inventoryitemid { get => _inventoryitemid; set => _inventoryitemid = value; }

    private string? _remarks;
    private string? Remarks { get => _remarks; set => _remarks = value; }

    public virtual Inventoryitem Inventoryitem { get; private set; } = null!;

    public virtual Loanlist Loanlist { get; private set; } = null!;
}
