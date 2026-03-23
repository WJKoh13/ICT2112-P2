namespace ProRental.Domain.Entities;

public partial class Train : Transport
{
    public int ReadTrainId() => getTrainID();

    public string ReadTrainType() => getTrainType();

    public string ReadTrainNumber() => getTrainNumber();
}
