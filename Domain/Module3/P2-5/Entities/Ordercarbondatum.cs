// Partial extension of the scaffolded Ordercarbondata entity.
// DO NOT add EF Core attributes, navigation properties, or DB-mapped fields here.

namespace ProRental.Domain.Entities;

public partial class Ordercarbondatum
{
    // ── Convenience display helpers ──────────────────────────────────────────

    /// Returns a human-readable impact label derived from TotalCarbon if
    /// the DB ImpactLevel column is null.
    public string GetDisplayImpactLevel()
    {
        if (!string.IsNullOrWhiteSpace(Impactlevel))
            return Impactlevel;

        return Totalcarbon switch
        {
            <= 500  => "Low",
            <= 1500 => "Medium",
            _       => "High"
        };
    }

    /// Sums all carbon components as a sanity-check total.
    public double GetComputedTotal()
        => Productcarbon + Packagingcarbon + Staffcarbon + Buildingcarbon;
}
