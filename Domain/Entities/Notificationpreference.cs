using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Notificationpreference
{
    public int Preferenceid { get; set; }

    public int Userid { get; set; }

    public bool Emailenabled { get; set; }

    public bool Smsenabled { get; set; }

    public virtual User User { get; set; } = null!;
}
