using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Clearanceitem
{
    private int _clearanceitemid;
    private int Clearanceitemid { get => _clearanceitemid; set => _clearanceitemid = value; }

    private int _clearancebatchid;
    private int Clearancebatchid { get => _clearancebatchid; set => _clearancebatchid = value; }

    private int _inventoryitemid;
    private int Inventoryitemid { get => _inventoryitemid; set => _inventoryitemid = value; }

    private decimal? _finalprice;
    private decimal? Finalprice { get => _finalprice; set => _finalprice = value; }

    private decimal? _recommendedprice;
    private decimal? Recommendedprice { get => _recommendedprice; set => _recommendedprice = value; }

    private DateTime? _saledate;
    private DateTime? Saledate { get => _saledate; set => _saledate = value; }

    public virtual Clearancebatch Clearancebatch { get; private set; } = null!;

    public virtual Inventoryitem Inventoryitem { get; private set; } = null!;
}
