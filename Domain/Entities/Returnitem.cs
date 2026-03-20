using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Returnitem
{
    private int _returnitemid;
    private int Returnitemid { get => _returnitemid; set => _returnitemid = value; }

    private int _returnrequestid;
    private int Returnrequestid { get => _returnrequestid; set => _returnrequestid = value; }

    private int _inventoryitemid;
    private int Inventoryitemid { get => _inventoryitemid; set => _inventoryitemid = value; }

    private DateTime? _completiondate;
    private DateTime? Completiondate { get => _completiondate; set => _completiondate = value; }

    private string? _image;
    private string? Image { get => _image; set => _image = value; }

    public virtual ICollection<Damagereport> Damagereports { get; private set; } = new List<Damagereport>();

    public virtual Inventoryitem Inventoryitem { get; private set; } = null!;

    public virtual Returnrequest Returnrequest { get; private set; } = null!;
}
