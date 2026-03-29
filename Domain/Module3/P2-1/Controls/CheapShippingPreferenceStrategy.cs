using ProRental.Domain.Enums;
using ProRental.Interfaces.Module3.P2_1;

namespace ProRental.Domain.Controls;

/// <summary>
/// Preference strategy for budget-focused checkout shipping.
/// by: ernest
/// </summary>
public sealed class CheapShippingPreferenceStrategy : IShippingPreferenceStrategy
{
    public PreferenceType PreferenceType => PreferenceType.CHEAP;

    public IReadOnlyList<TransportMode> ResolveAllowedModes(bool isSameCountry)
    {
        return isSameCountry
            ? [TransportMode.TRUCK]
            : [TransportMode.SHIP];
    }
}
