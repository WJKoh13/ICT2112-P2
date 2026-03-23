using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Reliabilityrating
{
    private int _ratingid;
    public int Ratingid { get => _ratingid; set => _ratingid = value; }

    private int? _supplierid;
    public int? Supplierid { get => _supplierid; set => _supplierid = value; }

    private decimal? _score;
    public decimal? Score { get => _score; set => _score = value; }

    private string? _rationale;
    public string? Rationale { get => _rationale; set => _rationale = value; }

    private int? _calculatedbyuserid;
    public int? Calculatedbyuserid { get => _calculatedbyuserid; set => _calculatedbyuserid = value; }

    private DateTime? _calculatedat;
    public DateTime? Calculatedat { get => _calculatedat; set => _calculatedat = value; }

    public virtual Supplier? Supplier { get; private set; }

    public virtual ICollection<Vettingrecord> Vettingrecords { get; private set; } = new List<Vettingrecord>();
}
