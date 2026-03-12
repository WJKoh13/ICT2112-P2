using System;
using System.Collections.Generic;

namespace BaseApp.Data.Entities;

public partial class Product
{
    public int Productid { get; set; }

    public int Categoryid { get; set; }

    public string Sku { get; set; } = null!;

    public DateTime Createdat { get; set; }

    public DateTime Updatedat { get; set; }

    public virtual ICollection<Cartitem> Cartitems { get; set; } = new List<Cartitem>();

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<Inventoryitem> Inventoryitems { get; set; } = new List<Inventoryitem>();

    public virtual ICollection<Lineitem> Lineitems { get; set; } = new List<Lineitem>();

    public virtual ICollection<Orderitem> Orderitems { get; set; } = new List<Orderitem>();

    public virtual Productdetail? Productdetail { get; set; }

    public virtual ICollection<Productfootprint> Productfootprints { get; set; } = new List<Productfootprint>();
}
