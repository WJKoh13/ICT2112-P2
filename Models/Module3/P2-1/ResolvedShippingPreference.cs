using ProRental.Domain.Enums;

namespace ProRental.Models.Module3.P2_1;

/// <summary>
/// Internal preference-resolution result used by Feature 1 after a customer picks
/// a checkout shipping preference.
/// by: ernest
/// </summary>
public sealed record ResolvedShippingPreference(
    PreferenceType PreferenceType,
    int DisplayPreference,
    string DisplayName,
    string Description,
    int DeliveryDays,
    IReadOnlyList<TransportMode> AllowedModes);
