using ProRental.Interfaces.Module3;
using ProRental.Domain.Entities;

namespace ProRental.Controllers.Module3.P2_5;

public class OrganizationalCarbonFootprintControl : IOrganizationalFootprintControl
{
    public OrganizationalFootprint CreateOrganizationalFootprint(string organizationId, string organizationName, float volume, float toxicPercentage)
    {
        return new OrganizationalFootprint(organizationId, organizationName, volume, toxicPercentage);
    }
}