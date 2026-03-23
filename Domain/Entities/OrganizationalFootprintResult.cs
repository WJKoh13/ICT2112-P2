namespace ProRental.Domain.Entities;

public class OrganizationalFootprintResult
{
    public string BuildingCarbonFootprintId { get; set; }
    public float CarbonFootprint { get; set; }
    public string OrganizationId { get; set; }
    public string OrganizationName { get; set; }
}