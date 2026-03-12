using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Staff
{
    public int Staffid { get; set; }

    public int Userid { get; set; }

    public string Department { get; set; } = null!;

    public virtual ICollection<Staffaccesslog> Staffaccesslogs { get; set; } = new List<Staffaccesslog>();

    public virtual ICollection<Stafffootprint> Stafffootprints { get; set; } = new List<Stafffootprint>();

    public virtual User User { get; set; } = null!;
}
