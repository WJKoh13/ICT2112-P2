using System;
using System.Collections.Generic;

namespace BaseApp.Data.Entities;

public partial class Supplier
{
    public int Supplierid { get; set; }

    public string? Name { get; set; }

    public string? Details { get; set; }

    public int? Creditperiod { get; set; }

    public double? Avgturnaroundtime { get; set; }

    public bool? Isverified { get; set; }

    public virtual ICollection<Reliabilityrating> Reliabilityratings { get; set; } = new List<Reliabilityrating>();

    public virtual ICollection<Suppliercategorychangelog> Suppliercategorychangelogs { get; set; } = new List<Suppliercategorychangelog>();

    public virtual ICollection<Vettingrecord> Vettingrecords { get; set; } = new List<Vettingrecord>();
}
