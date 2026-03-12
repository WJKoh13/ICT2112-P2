using System;
using System.Collections.Generic;

namespace BaseApp.Data.Entities;

public partial class Returnlog
{
    public int Returnlogid { get; set; }

    public int Returnrequestid { get; set; }

    public int Rentalorderlogid { get; set; }

    public string? Customerid { get; set; }

    public DateTime? Requestdate { get; set; }

    public DateTime? Completiondate { get; set; }

    public string? Imageurl { get; set; }

    public string? Detailsjson { get; set; }

    public virtual Rentalorderlog Rentalorderlog { get; set; } = null!;

    public virtual Transactionlog ReturnlogNavigation { get; set; } = null!;

    public virtual Returnrequest Returnrequest { get; set; } = null!;
}
