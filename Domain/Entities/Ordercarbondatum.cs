using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Ordercarbondatum
{
    private int _ordercarbondataid;
    private int Ordercarbondataid { get => _ordercarbondataid; set => _ordercarbondataid = value; }

    private int _orderid;
    private int Orderid { get => _orderid; set => _orderid = value; }

    private double _productcarbon;
    private double Productcarbon { get => _productcarbon; set => _productcarbon = value; }

    private double _packagingcarbon;
    private double Packagingcarbon { get => _packagingcarbon; set => _packagingcarbon = value; }

    private double _staffcarbon;
    private double Staffcarbon { get => _staffcarbon; set => _staffcarbon = value; }

    private double _buildingcarbon;
    private double Buildingcarbon { get => _buildingcarbon; set => _buildingcarbon = value; }

    private double _totalcarbon;
    private double Totalcarbon { get => _totalcarbon; set => _totalcarbon = value; }

    private string? _impactlevel;
    private string? Impactlevel { get => _impactlevel; set => _impactlevel = value; }

    private DateTime _calculatedat;
    private DateTime Calculatedat { get => _calculatedat; set => _calculatedat = value; }

    public virtual ICollection<Customerreward> Customerrewards { get; private set; } = new List<Customerreward>();

    public virtual Order Order { get; private set; } = null!;
}
