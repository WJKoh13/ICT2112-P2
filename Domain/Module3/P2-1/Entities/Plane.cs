namespace ProRental.Domain.Entities;

public partial class Plane
{
    public int ReadTransportId() => getTransportId();

    public int ReadPlaneId() => getPlaneID();

    public string ReadPlaneType() => getPlaneType();

    public string ReadPlaneCallsign() => getPlaneCallsign();
}
