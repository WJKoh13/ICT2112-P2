using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Category
{
    public int Categoryid { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime Createddate { get; set; }

    public DateTime Updateddate { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
