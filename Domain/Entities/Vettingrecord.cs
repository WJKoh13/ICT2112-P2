using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Vettingrecord
{
    private int _vettingid;
    public int Vettingid { get => _vettingid; set => _vettingid = value; }

    private int? _ratingid;
    public int? Ratingid { get => _ratingid; set => _ratingid = value; }

    private int? _supplierid;
    public int? Supplierid { get => _supplierid; set => _supplierid = value; }

    private int? _vettedbyuserid;
    public int? Vettedbyuserid { get => _vettedbyuserid; set => _vettedbyuserid = value; }

    private DateTime? _vettedat;
    public DateTime? Vettedat { get => _vettedat; set => _vettedat = value; }

    private string? _notes;
    public string? Notes { get => _notes; set => _notes = value; }

    public virtual Reliabilityrating? Rating { get; private set; }

    public virtual Supplier? Supplier { get; private set; }
}
