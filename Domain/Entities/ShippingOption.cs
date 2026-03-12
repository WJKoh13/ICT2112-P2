using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class ShippingOption
{
    public int OptionId { get; set; }

    public string? DisplayName { get; set; }

    public decimal? Cost { get; set; }

    public double? CarbonFootprint { get; set; }

    public int? DeliveryDays { get; set; }

    public bool? IsGreenOption { get; set; }

    public int? RouteId { get; set; }

    public virtual Route? Route { get; set; }
}
