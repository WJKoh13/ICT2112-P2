using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class DeliveryBatch
{
    public int DeliveryBatchId { get; set; }

    public string? DestinationAddress { get; set; }

    public int? TotalOrders { get; set; }

    public double? CarbonSavings { get; set; }

    public int? SourceHubId { get; set; }

    public virtual ICollection<BatchOrder> BatchOrders { get; set; } = new List<BatchOrder>();

    public virtual ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();

    public virtual TransportationHub? SourceHub { get; set; }
}
