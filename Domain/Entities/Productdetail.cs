using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Productdetail
{
    private int _detailsid;
    public int Detailsid { get => _detailsid; set => _detailsid = value; }

    private int _productid;
    public int Productid { get => _productid; set => _productid = value; }

    private int _totalquantity;
    public int Totalquantity { get => _totalquantity; set => _totalquantity = value; }

    private string _name = null!;
    public string Name { get => _name; set => _name = value; }

    private string? _description;
    public string? Description { get => _description; set => _description = value; }

    private decimal? _weight;
    public decimal? Weight { get => _weight; set => _weight = value; }

    private string? _image;
    public string? Image { get => _image; set => _image = value; }

    private decimal _price;
    public decimal Price { get => _price; set => _price = value; }

    private decimal? _depositrate;
    public decimal? Depositrate { get => _depositrate; set => _depositrate = value; }

    public virtual Product Product { get; private set; } = null!;
}
