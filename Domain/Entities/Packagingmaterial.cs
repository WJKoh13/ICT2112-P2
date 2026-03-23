using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Packagingmaterial
{
    private int _materialid;
    public int Materialid { get => _materialid; set => _materialid = value; }

    private string _name = null!;
    public string Name { get => _name; set => _name = value; }

    private string? _type;
    public string? Type { get => _type; set => _type = value; }

    private bool _recyclable;
    public bool Recyclable { get => _recyclable; set => _recyclable = value; }

    private bool _reusable;
    public bool Reusable { get => _reusable; set => _reusable = value; }

    public virtual ICollection<Packagingconfigmaterial> Packagingconfigmaterials { get; private set; } = new List<Packagingconfigmaterial>();
}
