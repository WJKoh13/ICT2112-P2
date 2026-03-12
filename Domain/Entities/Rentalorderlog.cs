using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Rentalorderlog
{
    public int Rentalorderlogid { get; set; }

    public int? Orderid { get; set; }

    public int? Customerid { get; set; }

    public DateTime? Orderdate { get; set; }

    public decimal? Totalamount { get; set; }

    public string? Detailsjson { get; set; }

    public virtual ICollection<Loanlog> Loanlogs { get; set; } = new List<Loanlog>();

    public virtual Order? Order { get; set; }

    public virtual Transactionlog RentalorderlogNavigation { get; set; } = null!;

    public virtual ICollection<Returnlog> Returnlogs { get; set; } = new List<Returnlog>();
}
