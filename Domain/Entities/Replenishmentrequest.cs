using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Replenishmentrequest
{
    private int _requestid;
    public int Requestid { get => _requestid; set => _requestid = value; }

    private string? _requestedby;
    public string? Requestedby { get => _requestedby; set => _requestedby = value; }

    private DateTime? _createdat;
    public DateTime? Createdat { get => _createdat; set => _createdat = value; }

    private string? _remarks;
    public string? Remarks { get => _remarks; set => _remarks = value; }

    private DateTime? _completedat;
    public DateTime? Completedat { get => _completedat; set => _completedat = value; }

    private string? _completedby;
    public string? Completedby { get => _completedby; set => _completedby = value; }

    public virtual ICollection<Lineitem> Lineitems { get; private set; } = new List<Lineitem>();
}
