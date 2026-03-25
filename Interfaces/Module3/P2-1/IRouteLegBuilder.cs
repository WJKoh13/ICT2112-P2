using ProRental.Domain.Entities;
using ProRental.Domain.Enums;

namespace ProRental.Interfaces.Module3.P2_1;

public interface IRouteLegBuilder
{
    RouteLeg BuildFirstMileLeg(string startPoint, string endPoint);
    RouteLeg BuildMainTransportLeg(int sequence, string startPoint, string endPoint, TransportMode transportMode);
    RouteLeg BuildLastMileLeg(int sequence, string startPoint, string endPoint);
}
