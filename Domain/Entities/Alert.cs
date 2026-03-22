using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Alert
{
    private int _alertid;
    public int Alertid { get => _alertid; set => _alertid = value; }

    private int _productid;
    public int Productid { get => _productid; set => _productid = value; }

    private int _minthreshold;
    public int Minthreshold { get => _minthreshold; set => _minthreshold = value; }

    private int _currentstock;
    public int Currentstock { get => _currentstock; set => _currentstock = value; }

    private string _message = null!;
    public string Message { get => _message; set => _message = value; }

    private DateTime _createdat;
    public DateTime Createdat { get => _createdat; set => _createdat = value; }

    private DateTime _updatedat;
    public DateTime Updatedat { get => _updatedat; set => _updatedat = value; }

    public virtual Product Product { get; private set; } = null!;
}
