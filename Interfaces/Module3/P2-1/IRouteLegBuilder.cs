using ProRental.Domain.Entities;
using ProRental.Domain.Enums;

namespace ProRental.Interfaces.Module3.P2_1;

public interface IRouteLegBuilder
{
    Task<RouteLeg> BuildFirstMileLegAsync(TransportationHub warehouse, TransportationHub originHub, TransportMode transportMode);
    Task<RouteLeg> BuildMainTransportLegAsync(TransportationHub originHub, TransportationHub destinationHub, TransportMode transportMode);
    Task<RouteLeg> BuildLastMileLegAsync(TransportationHub destinationHub, string customerAddress, TransportMode transportMode);
}
