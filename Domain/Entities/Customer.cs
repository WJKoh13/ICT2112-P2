using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Customer
{
    public int Customerid { get; set; }

    public int Userid { get; set; }

    public string Address { get; set; } = null!;

    public int Customertype { get; set; }

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual ICollection<Checkout> Checkouts { get; set; } = new List<Checkout>();

    public virtual ICollection<CustomerChoice> CustomerChoices { get; set; } = new List<CustomerChoice>();

    public virtual ICollection<Customerreward> Customerrewards { get; set; } = new List<Customerreward>();

    public virtual ICollection<Loanlist> Loanlists { get; set; } = new List<Loanlist>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Refund> Refunds { get; set; } = new List<Refund>();

    public virtual ICollection<Returnrequest> Returnrequests { get; set; } = new List<Returnrequest>();

    public virtual User User { get; set; } = null!;
}
