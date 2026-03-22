using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Shipment
{
    private int _trackingid;
    public int Trackingid { get => _trackingid; set => _trackingid = value; }

    private int _orderid;
    public int Orderid { get => _orderid; set => _orderid = value; }

    private int _batchid;
    public int Batchid { get => _batchid; set => _batchid = value; }

    private double _weight;
    public double Weight { get => _weight; set => _weight = value; }

    private string _destination = null!;
    public string Destination { get => _destination; set => _destination = value; }

    public virtual DeliveryBatch Batch { get; private set; } = null!;

    public virtual Order Order { get; private set; } = null!;
}
