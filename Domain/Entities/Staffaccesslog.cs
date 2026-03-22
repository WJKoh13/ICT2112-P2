using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Staffaccesslog
{
    private int _accessid;
    public int Accessid { get => _accessid; set => _accessid = value; }

    private int _staffid;
    public int Staffid { get => _staffid; set => _staffid = value; }

    private DateTime _eventtime;
    public DateTime Eventtime { get => _eventtime; set => _eventtime = value; }

    public virtual Staff Staff { get; private set; } = null!;
}
