using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Suppliercategorychangelog
{
    public int Logid { get; set; }

    public int? Supplierid { get; set; }

    public string? Changereason { get; set; }

    public DateTime? Changedat { get; set; }

    public virtual Supplier? Supplier { get; set; }
}
