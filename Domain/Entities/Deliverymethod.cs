using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Deliverymethod
{
    public int Deliveryid { get; set; }

    public int Orderid { get; set; }

    public int Durationdays { get; set; }

    public decimal Deliverycost { get; set; }

    public string Carrierid { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;
}
