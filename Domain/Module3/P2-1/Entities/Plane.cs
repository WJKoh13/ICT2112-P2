namespace ProRental.Domain.Entities;

public partial class Plane : Transport
{
    public int ReadPlaneId() => getPlaneID();

    public string ReadPlaneType() => getPlaneType();

    public string ReadPlaneCallsign() => getPlaneCallsign();
}
