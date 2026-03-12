using System;
using System.Collections.Generic;

namespace BaseApp.Data.Entities;

public partial class User
{
    public int Userid { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Passwordhash { get; set; } = null!;

    public int? Phonecountry { get; set; }

    public string? Phonenumber { get; set; }

    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();

    public virtual ICollection<Notificationpreference> Notificationpreferences { get; set; } = new List<Notificationpreference>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();

    public virtual ICollection<Staff> Staff { get; set; } = new List<Staff>();
}
