using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Deposit
{
    public string Depositid { get; set; } = null!;

    public int Orderid { get; set; }

    public int Transactionid { get; set; }

    public decimal Originalamount { get; set; }

    public decimal Heldamount { get; set; }

    public decimal? Refundedamount { get; set; }

    public decimal? Forfeitedamount { get; set; }

    public DateTime Createdat { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual Transaction Transaction { get; set; } = null!;
}
