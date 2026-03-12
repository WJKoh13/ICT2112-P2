using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class ProductReturn
{
    public int ReturnId { get; set; }

    public string? ReturnStatus { get; set; }

    public double? TotalCarbon { get; set; }

    public DateOnly? DateIn { get; set; }

    public DateOnly? DateOn { get; set; }

    public virtual ICollection<ReturnStage> ReturnStages { get; set; } = new List<ReturnStage>();
}
