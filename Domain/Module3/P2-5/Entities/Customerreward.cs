// Partial extension of the scaffolded Customerreward entity.
// DO NOT add EF Core attributes, navigation properties, or DB-mapped fields here.
// Only add domain helper methods that operate on the existing scaffolded properties.

namespace ProRental.Domain.Entities;

public partial class Customerreward
{
    // ── Convenience display helpers ──────────────────────────────────────────

    /// Returns a formatted string representation of the reward value.
    public string GetFormattedRewardValue()
        => Rewardtype?.ToLower() == "discount"
            ? $"{Rewardvalue:F1}% off"
            : $"{Rewardvalue:F2} pts";

    /// Returns true if this reward was created within the last 30 days.
    public bool IsRecent()
        => Createdat >= DateTime.UtcNow.AddDays(-30);
}
