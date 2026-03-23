using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Notificationpreference
{
    private int _preferenceid;
    public int Preferenceid { get => _preferenceid; set => _preferenceid = value; }

    private int _userid;
    public int Userid { get => _userid; set => _userid = value; }

    private bool _emailenabled;
    public bool Emailenabled { get => _emailenabled; set => _emailenabled = value; }

    private bool _smsenabled;
    public bool Smsenabled { get => _smsenabled; set => _smsenabled = value; }

    public virtual User User { get; private set; } = null!;
}
