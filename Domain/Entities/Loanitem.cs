using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Loanitem
{
    private int _loanitemid;
    public int Loanitemid { get => _loanitemid; set => _loanitemid = value; }

    private int _loanlistid;
    public int Loanlistid { get => _loanlistid; set => _loanlistid = value; }

    private int _inventoryitemid;
    public int Inventoryitemid { get => _inventoryitemid; set => _inventoryitemid = value; }

    private string? _remarks;
    public string? Remarks { get => _remarks; set => _remarks = value; }

    public virtual Inventoryitem Inventoryitem { get; private set; } = null!;

    public virtual Loanlist Loanlist { get; private set; } = null!;
}
