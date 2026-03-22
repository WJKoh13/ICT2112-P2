using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Returnlog
{
    private int _returnlogid;
    public int Returnlogid { get => _returnlogid; set => _returnlogid = value; }

    private int _returnrequestid;
    public int Returnrequestid { get => _returnrequestid; set => _returnrequestid = value; }

    private int _rentalorderlogid;
    public int Rentalorderlogid { get => _rentalorderlogid; set => _rentalorderlogid = value; }

    private string? _customerid;
    public string? Customerid { get => _customerid; set => _customerid = value; }

    private DateTime? _requestdate;
    public DateTime? Requestdate { get => _requestdate; set => _requestdate = value; }

    private DateTime? _completiondate;
    public DateTime? Completiondate { get => _completiondate; set => _completiondate = value; }

    private string? _imageurl;
    public string? Imageurl { get => _imageurl; set => _imageurl = value; }

    private string? _detailsjson;
    public string? Detailsjson { get => _detailsjson; set => _detailsjson = value; }

    public virtual Rentalorderlog Rentalorderlog { get; private set; } = null!;

    public virtual Transactionlog ReturnlogNavigation { get; private set; } = null!;

    public virtual Returnrequest Returnrequest { get; private set; } = null!;
}
