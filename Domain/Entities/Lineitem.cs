using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Lineitem
{
    private int _lineitemid;
    public int Lineitemid { get => _lineitemid; set => _lineitemid = value; }

    private int? _requestid;
    public int? Requestid { get => _requestid; set => _requestid = value; }

    private int? _productid;
    public int? Productid { get => _productid; set => _productid = value; }

    private int? _quantityrequest;
    public int? Quantityrequest { get => _quantityrequest; set => _quantityrequest = value; }

    private string? _remarks;
    public string? Remarks { get => _remarks; set => _remarks = value; }

    public virtual Product? Product { get; private set; }

    public virtual Replenishmentrequest? Request { get; private set; }
}
