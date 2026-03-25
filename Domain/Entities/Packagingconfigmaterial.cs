using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Packagingconfigmaterial
{
    private int _configurationid;
    private int Configurationid { get => _configurationid; set => _configurationid = value; }

    private int _materialid;
    private int Materialid { get => _materialid; set => _materialid = value; }

    private string? _category;
    private string? Category { get => _category; set => _category = value; }

    private int _quantity;
    private int Quantity { get => _quantity; set => _quantity = value; }

    public virtual Packagingconfiguration Configuration { get; private set; } = null!;

    public virtual Packagingmaterial Material { get; private set; } = null!;
}
