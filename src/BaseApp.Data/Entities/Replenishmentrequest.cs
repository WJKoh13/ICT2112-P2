using System;
using System.Collections.Generic;

namespace BaseApp.Data.Entities;

public partial class Replenishmentrequest
{
    public int Requestid { get; set; }

    public string? Requestedby { get; set; }

    public DateTime? Createdat { get; set; }

    public string? Remarks { get; set; }

    public DateTime? Completedat { get; set; }

    public string? Completedby { get; set; }

    public virtual ICollection<Lineitem> Lineitems { get; set; } = new List<Lineitem>();
}
