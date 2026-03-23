namespace ProRental.Domain.Entities;

public partial class Ship
{
    public int ReadTransportId() => getTransportId();

    public int ReadShipId() => getShipID();

    public string ReadVesselType() => getVesselType();

    public string ReadVesselNumber() => getVesselNumber();

    public string ReadMaxVesselSize() => getMaxVesselSize();
}
