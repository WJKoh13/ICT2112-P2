using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Category
{
    private int _categoryid;
    public int Categoryid { get => _categoryid; set => _categoryid = value; }

    private string _name = null!;
    public string Name { get => _name; set => _name = value; }

    private string? _description;
    public string? Description { get => _description; set => _description = value; }

    private DateTime _createddate;
    public DateTime Createddate { get => _createddate; set => _createddate = value; }

    private DateTime _updateddate;
    public DateTime Updateddate { get => _updateddate; set => _updateddate = value; }

    public virtual ICollection<Product> Products { get; private set; } = new List<Product>();
}
