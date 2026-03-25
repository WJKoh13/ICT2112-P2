using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Shipment
{
    private int _trackingid;
    private int Trackingid { get => _trackingid; set => _trackingid = value; }

    private int _orderid;
    private int Orderid { get => _orderid; set => _orderid = value; }

    private int _batchid;
    private int Batchid { get => _batchid; set => _batchid = value; }

    private double _weight;
    private double Weight { get => _weight; set => _weight = value; }

    private string _destination = null!;
    private string Destination { get => _destination; set => _destination = value; }

    public virtual DeliveryBatch Batch { get; private set; } = null!;

    public virtual Order Order { get; private set; } = null!;
}
