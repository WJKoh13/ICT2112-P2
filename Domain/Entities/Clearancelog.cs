using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Clearancelog
{
    public int Clearancelogid { get; set; }

    public int Clearancebatchid { get; set; }

    public string? Batchname { get; set; }

    public DateTime? Clearancedate { get; set; }

    public string? Detailsjson { get; set; }

    public virtual Clearancebatch Clearancebatch { get; set; } = null!;

    public virtual Transactionlog ClearancelogNavigation { get; set; } = null!;
}
