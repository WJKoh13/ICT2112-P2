using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Packagingmaterial
{
    private int _materialid;
    private int Materialid { get => _materialid; set => _materialid = value; }

    private string _name = null!;
    private string Name { get => _name; set => _name = value; }

    private string? _type;
    private string? Type { get => _type; set => _type = value; }

    private bool _recyclable;
    private bool Recyclable { get => _recyclable; set => _recyclable = value; }

    private bool _reusable;
    private bool Reusable { get => _reusable; set => _reusable = value; }

    public virtual ICollection<Packagingconfigmaterial> Packagingconfigmaterials { get; private set; } = new List<Packagingconfigmaterial>();
}
