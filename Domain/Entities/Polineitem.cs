using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Polineitem
{
    private int _polineid;
    private int Polineid { get => _polineid; set => _polineid = value; }

    private int? _poid;
    private int? Poid { get => _poid; set => _poid = value; }

    private int? _productid;
    private int? Productid { get => _productid; set => _productid = value; }

    private int? _qty;
    private int? Qty { get => _qty; set => _qty = value; }

    private decimal? _unitprice;
    private decimal? Unitprice { get => _unitprice; set => _unitprice = value; }

    private decimal? _linetotal;
    private decimal? Linetotal { get => _linetotal; set => _linetotal = value; }

    public virtual Purchaseorder? Po { get; private set; }

    public virtual Stockitem? Product { get; private set; }
}
