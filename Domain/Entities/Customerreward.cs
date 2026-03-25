using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Customerreward
{
    private int _rewardid;
    private int Rewardid { get => _rewardid; set => _rewardid = value; }

    private int _customerid;
    private int Customerid { get => _customerid; set => _customerid = value; }

    private int _ordercarbondataid;
    private int Ordercarbondataid { get => _ordercarbondataid; set => _ordercarbondataid = value; }

    private string _rewardtype = null!;
    private string Rewardtype { get => _rewardtype; set => _rewardtype = value; }

    private double _rewardvalue;
    private double Rewardvalue { get => _rewardvalue; set => _rewardvalue = value; }

    private DateTime _createdat;
    private DateTime Createdat { get => _createdat; set => _createdat = value; }

    public virtual Customer Customer { get; private set; } = null!;

    public virtual Ordercarbondatum Ordercarbondata { get; private set; } = null!;
}
