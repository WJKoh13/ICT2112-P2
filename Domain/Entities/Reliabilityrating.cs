using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Reliabilityrating
{
    public int Ratingid { get; set; }

    public int? Supplierid { get; set; }

    public decimal? Score { get; set; }

    public string? Rationale { get; set; }

    public int? Calculatedbyuserid { get; set; }

    public DateTime? Calculatedat { get; set; }

    public virtual Supplier? Supplier { get; set; }

    public virtual ICollection<Vettingrecord> Vettingrecords { get; set; } = new List<Vettingrecord>();
}
