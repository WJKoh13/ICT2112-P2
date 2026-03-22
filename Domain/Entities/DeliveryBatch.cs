using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class DeliveryBatch
{
    private int _deliveryBatchId;
    public int DeliveryBatchId { get => _deliveryBatchId; set => _deliveryBatchId = value; }

    private double? _batchWeightKg;
    public double? BatchWeightKg { get => _batchWeightKg; set => _batchWeightKg = value; }

    private string? _destinationAddress;
    public string? DestinationAddress { get => _destinationAddress; set => _destinationAddress = value; }

    private int? _totalOrders;
    public int? TotalOrders { get => _totalOrders; set => _totalOrders = value; }

    private double? _carbonSavings;
    public double? CarbonSavings { get => _carbonSavings; set => _carbonSavings = value; }

    private int? _hubId;
    public int? HubId { get => _hubId; set => _hubId = value; }

    public virtual ICollection<BatchOrder> BatchOrders { get; private set; } = new List<BatchOrder>();

    public virtual TransportationHub? Hub { get; private set; }

    public virtual ICollection<Shipment> Shipments { get; private set; } = new List<Shipment>();
}
