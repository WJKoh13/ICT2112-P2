using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Clearancebatch
{
    private int _clearancebatchid;
    public int Clearancebatchid { get => _clearancebatchid; set => _clearancebatchid = value; }

    private string _batchname = null!;
    public string Batchname { get => _batchname; set => _batchname = value; }

    private DateTime _createddate;
    public DateTime Createddate { get => _createddate; set => _createddate = value; }

    private DateTime? _clearancedate;
    public DateTime? Clearancedate { get => _clearancedate; set => _clearancedate = value; }

    public virtual ICollection<Clearanceitem> Clearanceitems { get; private set; } = new List<Clearanceitem>();

    public virtual ICollection<Clearancelog> Clearancelogs { get; private set; } = new List<Clearancelog>();
}
