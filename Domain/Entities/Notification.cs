using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Notification
{
    private int _notificationid;
    public int Notificationid { get => _notificationid; set => _notificationid = value; }

    private int _userid;
    public int Userid { get => _userid; set => _userid = value; }

    private string _message = null!;
    public string Message { get => _message; set => _message = value; }

    private DateTime _datesent;
    public DateTime Datesent { get => _datesent; set => _datesent = value; }

    private bool _isread;
    public bool Isread { get => _isread; set => _isread = value; }

    public virtual User User { get; private set; } = null!;
}
