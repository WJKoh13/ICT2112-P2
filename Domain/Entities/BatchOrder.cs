using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class BatchOrder
{
    private int _batchId;
    public int BatchId { get => _batchId; set => _batchId = value; }

    private int _orderId;
    public int OrderId { get => _orderId; set => _orderId = value; }

    private DateTime? _addedTimestamp;
    public DateTime? AddedTimestamp { get => _addedTimestamp; set => _addedTimestamp = value; }

    public virtual DeliveryBatch Batch { get; private set; } = null!;

    public virtual Order Order { get; private set; } = null!;
}
