using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Session
{
    private int _sessionid;
    public int Sessionid { get => _sessionid; set => _sessionid = value; }

    private int _userid;
    public int Userid { get => _userid; set => _userid = value; }

    private string _role = null!;
    public string Role { get => _role; set => _role = value; }

    private DateTime _createdat;
    public DateTime Createdat { get => _createdat; set => _createdat = value; }

    private DateTime _expiresat;
    public DateTime Expiresat { get => _expiresat; set => _expiresat = value; }

    public virtual ICollection<Cart> Carts { get; private set; } = new List<Cart>();

    public virtual User User { get; private set; } = null!;
}
