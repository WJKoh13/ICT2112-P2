namespace ProRental.Domain.Entities;

public class OrganizationalFootprint
{
    public string OrganizationId { get; set; }
    public string OrganizationName { get; set; }
    public float Volume { get; set; }
    public float ToxicPercentage { get; set; }

    public OrganizationalFootprint(string organizationId, string organizationName, float volume, float toxicPercentage)
    {
        OrganizationId = organizationId;
        OrganizationName = organizationName;
        Volume = volume;
        ToxicPercentage = toxicPercentage;
    }
}