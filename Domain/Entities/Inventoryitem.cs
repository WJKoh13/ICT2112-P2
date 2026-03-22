using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Inventoryitem
{
    private int _inventoryid;
    public int Inventoryid { get => _inventoryid; set => _inventoryid = value; }

    private int _productid;
    public int Productid { get => _productid; set => _productid = value; }

    private string _serialnumber = null!;
    public string Serialnumber { get => _serialnumber; set => _serialnumber = value; }

    private DateTime _createdat;
    public DateTime Createdat { get => _createdat; set => _createdat = value; }

    private DateTime _updatedat;
    public DateTime Updatedat { get => _updatedat; set => _updatedat = value; }

    private DateTime? _expirydate;
    public DateTime? Expirydate { get => _expirydate; set => _expirydate = value; }

    public virtual Clearanceitem? Clearanceitem { get; private set; }

    public virtual ICollection<Loanitem> Loanitems { get; private set; } = new List<Loanitem>();

    public virtual Product Product { get; private set; } = null!;

    public virtual ICollection<Returnitem> Returnitems { get; private set; } = new List<Returnitem>();
}
