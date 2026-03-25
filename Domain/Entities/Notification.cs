using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Notification
{
    private int _notificationid;
    private int Notificationid { get => _notificationid; set => _notificationid = value; }

    private int _userid;
    private int Userid { get => _userid; set => _userid = value; }

    private string _message = null!;
    private string Message { get => _message; set => _message = value; }

    private DateTime _datesent;
    private DateTime Datesent { get => _datesent; set => _datesent = value; }

    private bool _isread;
    private bool Isread { get => _isread; set => _isread = value; }

    public virtual User User { get; private set; } = null!;
}
