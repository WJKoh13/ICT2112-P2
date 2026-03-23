namespace ProRental.Domain.Entities;

public partial class Truck : Transport
{
    public int ReadTruckId() => getTruckID();

    public string ReadTruckType() => getTruckType();

    public string ReadLicensePlate() => getLicensePlate();
}
