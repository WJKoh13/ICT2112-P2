namespace ProRental.Domain.Entities;

public class OrganizationalFootprint
{
    private string _organizationalFootprintId;
    private string _organizationalId;
    private string _organizationName;
    private float _volume;
    private float _toxicPercentage;

    public OrganizationalFootprint(string organizationId, string organizationName, float volume, float toxicPercentage)
    {
        _organizationalFootprintId = Guid.NewGuid().ToString();
        _organizationalId = organizationId;
        _organizationName = organizationName;
        _volume = volume;
        _toxicPercentage = toxicPercentage;
    }

    public float GetVolume()
    {
        return _volume;
    }

    private void SetVolume(float volume)
    {
        _volume = volume;
    }

    public float GetToxicPercentage()
    {
        return _toxicPercentage;
    }

    private void SetToxicPercentage(float percentage)
    {
        _toxicPercentage = percentage;
    }
}