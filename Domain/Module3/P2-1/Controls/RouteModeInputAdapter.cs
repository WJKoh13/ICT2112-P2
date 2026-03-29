using ProRental.Domain.Enums;
using ProRental.Models.Module3.P2_1;

namespace ProRental.Domain.Module3.P2_1.Controls;

internal static class RouteModeInputAdapter
{
    public static RouteModeProfile ResolveProfile(IReadOnlyList<TransportMode> modes)
    {
        ArgumentNullException.ThrowIfNull(modes);

        return modes.Count switch
        {
            1 => ResolveLegacySingleModeProfile(modes[0]),
            2 => ResolveLegacyMultiModeProfile(modes),
            3 => new RouteModeProfile(modes[0], modes[1], modes[2], UseThreeLegRoute: true),
            _ => throw new ArgumentException(
                "Route creation expects either a legacy one/two-mode request or an explicit three-leg mode profile.",
                nameof(modes))
        };
    }

    private static RouteModeProfile ResolveLegacySingleModeProfile(TransportMode mainTransportMode)
    {
        return mainTransportMode switch
        {
            TransportMode.PLANE or TransportMode.SHIP => new RouteModeProfile(
                TransportMode.TRUCK,
                mainTransportMode,
                TransportMode.TRUCK,
                UseThreeLegRoute: true),
            TransportMode.TRAIN or TransportMode.TRUCK => new RouteModeProfile(
                mainTransportMode,
                mainTransportMode,
                mainTransportMode,
                UseThreeLegRoute: false),
            _ => throw new RouteResolutionException($"Transport mode '{mainTransportMode}' is not supported for route generation.")
        };
    }

    private static RouteModeProfile ResolveLegacyMultiModeProfile(IReadOnlyList<TransportMode> modes)
    {
        var mainTransportMode = modes[^1];
        return ResolveLegacySingleModeProfile(mainTransportMode);
    }
}
