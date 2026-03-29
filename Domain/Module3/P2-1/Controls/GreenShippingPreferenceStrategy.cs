using ProRental.Domain.Enums;
using ProRental.Interfaces.Module3.P2_1;

namespace ProRental.Domain.Controls;

/// <summary>
/// Preference strategy for lower-impact checkout shipping.
/// by: ernest
/// </summary>
public sealed class GreenShippingPreferenceStrategy : IShippingPreferenceStrategy
{
    public PreferenceType PreferenceType => PreferenceType.GREEN;

    public IReadOnlyList<TransportMode> ResolveAllowedModes(bool isSameCountry)
    {
        return isSameCountry
            ? [TransportMode.TRAIN]
            : [TransportMode.TRAIN, TransportMode.SHIP];
    }
}
