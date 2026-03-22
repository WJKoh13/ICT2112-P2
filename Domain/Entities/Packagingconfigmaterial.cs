using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Packagingconfigmaterial
{
    private int _configurationid;
    public int Configurationid { get => _configurationid; set => _configurationid = value; }

    private int _materialid;
    public int Materialid { get => _materialid; set => _materialid = value; }

    private string? _category;
    public string? Category { get => _category; set => _category = value; }

    private int _quantity;
    public int Quantity { get => _quantity; set => _quantity = value; }

    public virtual Packagingconfiguration Configuration { get; private set; } = null!;

    public virtual Packagingmaterial Material { get; private set; } = null!;
}
