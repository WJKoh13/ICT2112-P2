using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class CustomerChoice
{
    public int CustomerId { get; set; }

    public int OrderId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Customer Customer { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;
}
