using System;
using System.Collections.Generic;

namespace BaseApp.Data.Entities;

public partial class Polineitem
{
    public int Polineid { get; set; }

    public int? Poid { get; set; }

    public int? Productid { get; set; }

    public int? Qty { get; set; }

    public decimal? Unitprice { get; set; }

    public decimal? Linetotal { get; set; }

    public virtual Purchaseorder? Po { get; set; }

    public virtual Stockitem? Product { get; set; }
}
