using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Damagereport
{
    private int _damagereportid;
    public int Damagereportid { get => _damagereportid; set => _damagereportid = value; }

    private int _returnitemid;
    public int Returnitemid { get => _returnitemid; set => _returnitemid = value; }

    private string? _description;
    public string? Description { get => _description; set => _description = value; }

    private string? _severity;
    public string? Severity { get => _severity; set => _severity = value; }

    private decimal? _repaircost;
    public decimal? Repaircost { get => _repaircost; set => _repaircost = value; }

    private string? _images;
    public string? Images { get => _images; set => _images = value; }

    private DateTime _reportdate;
    public DateTime Reportdate { get => _reportdate; set => _reportdate = value; }

    public virtual Returnitem Returnitem { get; private set; } = null!;
}
