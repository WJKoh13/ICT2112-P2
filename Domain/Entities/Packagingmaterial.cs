using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Packagingmaterial
{
    public int Materialid { get; set; }

    public string Name { get; set; } = null!;

    public string? Type { get; set; }

    public bool Recyclable { get; set; }

    public bool Reusable { get; set; }

    public virtual ICollection<Packagingconfigmaterial> Packagingconfigmaterials { get; set; } = new List<Packagingconfigmaterial>();
}
