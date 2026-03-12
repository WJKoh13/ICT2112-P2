using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Staffaccesslog
{
    public int Accessid { get; set; }

    public int Staffid { get; set; }

    public DateTime Eventtime { get; set; }

    public virtual Staff Staff { get; set; } = null!;
}
