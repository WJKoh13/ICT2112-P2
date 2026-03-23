using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Supplier
{
    private int _supplierid;
    public int Supplierid { get => _supplierid; set => _supplierid = value; }

    private string? _name;
    public string? Name { get => _name; set => _name = value; }

    private string? _details;
    public string? Details { get => _details; set => _details = value; }

    private int? _creditperiod;
    public int? Creditperiod { get => _creditperiod; set => _creditperiod = value; }

    private double? _avgturnaroundtime;
    public double? Avgturnaroundtime { get => _avgturnaroundtime; set => _avgturnaroundtime = value; }

    private bool? _isverified;
    public bool? Isverified { get => _isverified; set => _isverified = value; }

    public virtual ICollection<Reliabilityrating> Reliabilityratings { get; private set; } = new List<Reliabilityrating>();

    public virtual ICollection<Suppliercategorychangelog> Suppliercategorychangelogs { get; private set; } = new List<Suppliercategorychangelog>();

    public virtual ICollection<Vettingrecord> Vettingrecords { get; private set; } = new List<Vettingrecord>();
}
