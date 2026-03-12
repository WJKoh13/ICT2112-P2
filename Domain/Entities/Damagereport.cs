using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Damagereport
{
    public int Damagereportid { get; set; }

    public int Returnitemid { get; set; }

    public string? Description { get; set; }

    public string? Severity { get; set; }

    public decimal? Repaircost { get; set; }

    public string? Images { get; set; }

    public DateTime Reportdate { get; set; }

    public virtual Returnitem Returnitem { get; set; } = null!;
}
