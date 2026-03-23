using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Ordercarbondatum
{
    private int _ordercarbondataid;
    public int Ordercarbondataid { get => _ordercarbondataid; set => _ordercarbondataid = value; }

    private int _orderid;
    public int Orderid { get => _orderid; set => _orderid = value; }

    private double _productcarbon;
    public double Productcarbon { get => _productcarbon; set => _productcarbon = value; }

    private double _packagingcarbon;
    public double Packagingcarbon { get => _packagingcarbon; set => _packagingcarbon = value; }

    private double _staffcarbon;
    public double Staffcarbon { get => _staffcarbon; set => _staffcarbon = value; }

    private double _buildingcarbon;
    public double Buildingcarbon { get => _buildingcarbon; set => _buildingcarbon = value; }

    private double _totalcarbon;
    public double Totalcarbon { get => _totalcarbon; set => _totalcarbon = value; }

    private string? _impactlevel;
    public string? Impactlevel { get => _impactlevel; set => _impactlevel = value; }

    private DateTime _calculatedat;
    public DateTime Calculatedat { get => _calculatedat; set => _calculatedat = value; }

    public virtual ICollection<Customerreward> Customerrewards { get; private set; } = new List<Customerreward>();

    public virtual Order Order { get; private set; } = null!;
}
