using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Returnitem
{
    public int Returnitemid { get; set; }

    public int Returnrequestid { get; set; }

    public int Inventoryitemid { get; set; }

    public DateTime? Completiondate { get; set; }

    public string? Image { get; set; }

    public virtual ICollection<Damagereport> Damagereports { get; set; } = new List<Damagereport>();

    public virtual Inventoryitem Inventoryitem { get; set; } = null!;

    public virtual Returnrequest Returnrequest { get; set; } = null!;
}
