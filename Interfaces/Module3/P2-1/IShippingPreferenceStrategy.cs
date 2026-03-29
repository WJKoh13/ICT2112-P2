using ProRental.Domain.Enums;

namespace ProRental.Interfaces.Module3.P2_1;

/// <summary>
/// Pure strategy contract for resolving allowed transport modes for one
/// checkout shipping preference.
/// by: ernest
/// </summary>
public interface IShippingPreferenceStrategy
{
    PreferenceType PreferenceType { get; }

    IReadOnlyList<TransportMode> ResolveAllowedModes(bool isSameCountry);
}
