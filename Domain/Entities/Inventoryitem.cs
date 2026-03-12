using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Inventoryitem
{
    public int Inventoryid { get; set; }

    public int Productid { get; set; }

    public string Serialnumber { get; set; } = null!;

    public DateTime Createdat { get; set; }

    public DateTime Updatedat { get; set; }

    public DateTime? Expirydate { get; set; }

    public virtual Clearanceitem? Clearanceitem { get; set; }

    public virtual ICollection<Loanitem> Loanitems { get; set; } = new List<Loanitem>();

    public virtual Product Product { get; set; } = null!;

    public virtual ICollection<Returnitem> Returnitems { get; set; } = new List<Returnitem>();
}
