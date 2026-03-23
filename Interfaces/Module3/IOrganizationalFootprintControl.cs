using ProRental.Domain.Entities;

namespace ProRental.Interfaces.Module3;

public interface IOrganizationalFootprintControl
{
    OrganizationalFootprint CreateOrganizationalFootprint(string organizationId, string organizationName, float volume, float toxicPercentage);
}