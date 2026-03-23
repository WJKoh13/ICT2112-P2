using ProRental.Domain.Enums;

namespace ProRental.Domain.Entities;

public partial class Transport
{
    private TransportMode? _transportMode;
    private TransportMode? TransportMode { get => _transportMode; set => _transportMode = value; }

    public int ReadTransportId() => getTransportId();

    public string ReadTransportationType() => getTransportationType();

    public float ReadMaxLoadKg() => getMaxLoadKG();

    public float ReadVehicleSizeM2() => getVehicleSizem2();

    public bool ReadIsAvailable() => getIsAvailable();
}
