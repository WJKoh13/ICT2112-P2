using ProRental.Domain.Enums;

namespace ProRental.Models.Module3.P2_1;

/// <summary>
/// Allowed routing modes for each customer-facing shipping preference.
/// by: ernest
/// </summary>
public static class PreferenceTypeModes
{
    public static RouteModeProfile ResolveRouteProfile(PreferenceType preferenceType, bool isSameCountry)
    {
        return preferenceType switch
        {
            PreferenceType.FAST when isSameCountry => new RouteModeProfile(
                TransportMode.TRAIN,
                TransportMode.TRAIN,
                TransportMode.TRAIN,
                UseThreeLegRoute: false),
            PreferenceType.CHEAP when isSameCountry => new RouteModeProfile(
                TransportMode.TRUCK,
                TransportMode.TRUCK,
                TransportMode.TRUCK,
                UseThreeLegRoute: false),
            PreferenceType.GREEN when isSameCountry => new RouteModeProfile(
                TransportMode.TRAIN,
                TransportMode.TRAIN,
                TransportMode.TRAIN,
                UseThreeLegRoute: false),
            PreferenceType.FAST => new RouteModeProfile(
                TransportMode.TRUCK,
                TransportMode.PLANE,
                TransportMode.TRUCK,
                UseThreeLegRoute: true),
            PreferenceType.CHEAP => new RouteModeProfile(
                TransportMode.TRUCK,
                TransportMode.SHIP,
                TransportMode.TRUCK,
                UseThreeLegRoute: true),
            _ => new RouteModeProfile(
                TransportMode.TRUCK,
                TransportMode.TRAIN,
                TransportMode.TRUCK,
                UseThreeLegRoute: true)
        };
    }

    public static IReadOnlyList<TransportMode> ResolveAllowedModes(PreferenceType preferenceType, bool isSameCountry)
    {
        return preferenceType switch
        {
            PreferenceType.FAST when isSameCountry => [TransportMode.TRAIN],
            PreferenceType.CHEAP when isSameCountry => [TransportMode.TRUCK],
            PreferenceType.GREEN when isSameCountry => [TransportMode.TRAIN],
            PreferenceType.FAST => [TransportMode.PLANE],
            PreferenceType.CHEAP => [TransportMode.SHIP],
            _ => [TransportMode.TRAIN, TransportMode.SHIP]
        };
    }

    public static string GetAllowedModesLabel(PreferenceType preferenceType, bool isSameCountry)
    {
        return string.Join(" + ", ResolveAllowedModes(preferenceType, isSameCountry));
    }
}
