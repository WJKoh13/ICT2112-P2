using ProRental.Domain.Enums;

namespace ProRental.Models.Module3.P2_1;

/// <summary>
/// Allowed routing modes for each customer-facing shipping preference.
/// by: ernest
/// </summary>
public static class PreferenceTypeModes
{
    public static readonly IReadOnlyDictionary<PreferenceType, List<TransportMode>> AllowedModes =
        new Dictionary<PreferenceType, List<TransportMode>>
        {
            [PreferenceType.FAST] = [TransportMode.PLANE],
            [PreferenceType.CHEAP] = [TransportMode.SHIP],
            [PreferenceType.GREEN] = [TransportMode.TRAIN]
        };
}
