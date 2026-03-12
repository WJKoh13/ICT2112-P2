using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Orderstatushistory
{
    public int Historyid { get; set; }

    public int Orderid { get; set; }

    public DateTime Timestamp { get; set; }

    public string Updatedby { get; set; } = null!;

    public string? Remark { get; set; }

    public virtual Order Order { get; set; } = null!;
}
