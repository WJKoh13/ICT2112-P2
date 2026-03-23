namespace ProRental.Domain.Entities;

public partial class Ordercarbondatum
{
    public string GetImpactBadgeColour() => Impactlevel switch
    {
        "Low"       => "success",
        "Moderate"  => "warning",
        "High"      => "danger",
        _           => "dark"
    };
}