using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Packagingconfiguration
{
    private int _configurationid;
    private int Configurationid { get => _configurationid; set => _configurationid = value; }

    private int _profileid;
    private int Profileid { get => _profileid; set => _profileid = value; }

    public virtual ICollection<Packagingconfigmaterial> Packagingconfigmaterials { get; private set; } = new List<Packagingconfigmaterial>();

    public virtual Packagingprofile Profile { get; private set; } = null!;
}
