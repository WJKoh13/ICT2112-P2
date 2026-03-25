using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Replenishmentrequest
{
    private int _requestid;
    private int Requestid { get => _requestid; set => _requestid = value; }

    private string? _requestedby;
    private string? Requestedby { get => _requestedby; set => _requestedby = value; }

    private DateTime? _createdat;
    private DateTime? Createdat { get => _createdat; set => _createdat = value; }

    private string? _remarks;
    private string? Remarks { get => _remarks; set => _remarks = value; }

    private DateTime? _completedat;
    private DateTime? Completedat { get => _completedat; set => _completedat = value; }

    private string? _completedby;
    private string? Completedby { get => _completedby; set => _completedby = value; }

    public virtual ICollection<Lineitem> Lineitems { get; private set; } = new List<Lineitem>();
}
