using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class Train
{
    private int _transportId;
    public int TransportId { get => _transportId; set => _transportId = value; }

    private int _trainId;
    public int TrainId { get => _trainId; set => _trainId = value; }

    private string? _trainType;
    public string? TrainType { get => _trainType; set => _trainType = value; }

    private string? _trainNumber;
    public string? TrainNumber { get => _trainNumber; set => _trainNumber = value; }

    public virtual Transport Transport { get; private set; } = null!;
}
