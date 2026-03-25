using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Damagereport
{
    private int _damagereportid;
    private int Damagereportid { get => _damagereportid; set => _damagereportid = value; }

    private int _returnitemid;
    private int Returnitemid { get => _returnitemid; set => _returnitemid = value; }

    private string? _description;
    private string? Description { get => _description; set => _description = value; }

    private string? _severity;
    private string? Severity { get => _severity; set => _severity = value; }

    private decimal? _repaircost;
    private decimal? Repaircost { get => _repaircost; set => _repaircost = value; }

    private string? _images;
    private string? Images { get => _images; set => _images = value; }

    private DateTime _reportdate;
    private DateTime Reportdate { get => _reportdate; set => _reportdate = value; }

    public virtual Returnitem Returnitem { get; private set; } = null!;
}
