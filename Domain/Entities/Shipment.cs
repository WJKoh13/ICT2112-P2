using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Shipment
{
    public int Trackingid { get; set; }

    public int Orderid { get; set; }

    public int Batchid { get; set; }

    public double Weight { get; set; }

    public string Destination { get; set; } = null!;

    public virtual DeliveryBatch Batch { get; set; } = null!;

    public virtual Order Order { get; set; } = null!;
}
