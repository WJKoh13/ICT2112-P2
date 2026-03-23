namespace ProRental.Domain.Entities;

public partial class Train
{
    public int ReadTransportId() => getTransportId();

    public int ReadTrainId() => getTrainID();

    public string ReadTrainType() => getTrainType();

    public string ReadTrainNumber() => getTrainNumber();
}
