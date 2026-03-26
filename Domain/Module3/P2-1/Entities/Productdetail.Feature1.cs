namespace ProRental.Domain.Entities;

/// <summary>
/// Feature 1 accessors for the shared Productdetail entity. Exposes the product
/// weight needed for carbon calculation inputs.
/// by: ernest
/// </summary>
public partial class Productdetail
{
    public double GetWeightKg() => (double?)_weight ?? 1d;
}
