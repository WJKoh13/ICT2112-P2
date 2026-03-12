using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Train
{
    public int TransportId { get; set; }

    public int TrainId { get; set; }

    public string? TrainType { get; set; }

    public string? LicensePlate { get; set; }

    public virtual Transport Transport { get; set; } = null!;
}
