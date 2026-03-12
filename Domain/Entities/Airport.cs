using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Airport
{
    public int HubId { get; set; }

    public string AirportCode { get; set; } = null!;

    public string AirportName { get; set; } = null!;

    public int? Terminal { get; set; }

    public virtual TransportationHub Hub { get; set; } = null!;
}
