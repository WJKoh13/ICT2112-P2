using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Session
{
    private int _sessionid;
    private int Sessionid { get => _sessionid; set => _sessionid = value; }

    private int _userid;
    private int Userid { get => _userid; set => _userid = value; }

    private string _role = null!;
    private string Role { get => _role; set => _role = value; }

    private DateTime _createdat;
    private DateTime Createdat { get => _createdat; set => _createdat = value; }

    private DateTime _expiresat;
    private DateTime Expiresat { get => _expiresat; set => _expiresat = value; }

    public virtual ICollection<Cart> Carts { get; private set; } = new List<Cart>();

    public virtual User User { get; private set; } = null!;
}
