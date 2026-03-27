using ProRental.Domain.Enums;

namespace ProRental.Models.Module3.P2_1;

public sealed record RouteModeProfile(
    TransportMode FirstMileMode,
    TransportMode MainTransportMode,
    TransportMode LastMileMode,
    bool UseThreeLegRoute)
{
    public List<TransportMode> ToModeList()
    {
        return UseThreeLegRoute
            ? [FirstMileMode, MainTransportMode, LastMileMode]
            : [MainTransportMode];
    }
}
