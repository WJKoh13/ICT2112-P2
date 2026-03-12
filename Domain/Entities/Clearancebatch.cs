using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Clearancebatch
{
    public int Clearancebatchid { get; set; }

    public string Batchname { get; set; } = null!;

    public DateTime Createddate { get; set; }

    public DateTime? Clearancedate { get; set; }

    public virtual ICollection<Clearanceitem> Clearanceitems { get; set; } = new List<Clearanceitem>();

    public virtual ICollection<Clearancelog> Clearancelogs { get; set; } = new List<Clearancelog>();
}
