using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Notification
{
    public int Notificationid { get; set; }

    public int Userid { get; set; }

    public string Message { get; set; } = null!;

    public DateTime Datesent { get; set; }

    public bool Isread { get; set; }

    public virtual User User { get; set; } = null!;
}
