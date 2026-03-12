using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Loanitem
{
    public int Loanitemid { get; set; }

    public int Loanlistid { get; set; }

    public int Inventoryitemid { get; set; }

    public string? Remarks { get; set; }

    public virtual Inventoryitem Inventoryitem { get; set; } = null!;

    public virtual Loanlist Loanlist { get; set; } = null!;
}
