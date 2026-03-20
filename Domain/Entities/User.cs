using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class User
{
    private int _userid;
    private int Userid { get => _userid; set => _userid = value; }

    private string _name = null!;
    private string Name { get => _name; set => _name = value; }

    private string _email = null!;
    private string Email { get => _email; set => _email = value; }

    private string _passwordhash = null!;
    private string Passwordhash { get => _passwordhash; set => _passwordhash = value; }

    private int? _phonecountry;
    private int? Phonecountry { get => _phonecountry; set => _phonecountry = value; }

    private string? _phonenumber;
    private string? Phonenumber { get => _phonenumber; set => _phonenumber = value; }

    public virtual Customer? Customer { get; private set; }

    public virtual ICollection<Notificationpreference> Notificationpreferences { get; private set; } = new List<Notificationpreference>();

    public virtual ICollection<Notification> Notifications { get; private set; } = new List<Notification>();

    public virtual ICollection<Session> Sessions { get; private set; } = new List<Session>();

    public virtual Staff? Staff { get; private set; }
}
