using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Staff
{
    private int _staffid;
    public int Staffid { get => _staffid; set => _staffid = value; }

    private int _userid;
    public int Userid { get => _userid; set => _userid = value; }

    private string _department = null!;
    public string Department { get => _department; set => _department = value; }

    public virtual ICollection<Staffaccesslog> Staffaccesslogs { get; private set; } = new List<Staffaccesslog>();

    public virtual ICollection<Stafffootprint> Stafffootprints { get; private set; } = new List<Stafffootprint>();

    public virtual User User { get; private set; } = null!;
}
