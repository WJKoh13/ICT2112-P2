using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Vettingrecord
{
    public int Vettingid { get; set; }

    public int? Ratingid { get; set; }

    public int? Supplierid { get; set; }

    public int? Vettedbyuserid { get; set; }

    public DateTime? Vettedat { get; set; }

    public string? Notes { get; set; }

    public virtual Reliabilityrating? Rating { get; set; }

    public virtual Supplier? Supplier { get; set; }
}
