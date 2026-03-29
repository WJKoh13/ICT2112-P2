using ProRental.Domain.Enums;
using ProRental.Interfaces.Module3.P2_1;

namespace ProRental.Domain.Controls;

/// <summary>
/// Preference strategy for time-sensitive checkout shipping.
/// by: ernest
/// </summary>
public sealed class FastShippingPreferenceStrategy : IShippingPreferenceStrategy
{
    public PreferenceType PreferenceType => PreferenceType.FAST;

    public IReadOnlyList<TransportMode> ResolveAllowedModes(bool isSameCountry)
    {
        return isSameCountry
            ? [TransportMode.TRAIN]
            : [TransportMode.PLANE];
    }
}
