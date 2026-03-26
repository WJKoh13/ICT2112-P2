using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class TrainStation : TransportationHub
{
    private string _trainstationCode = null!;
    private string TrainstationCode { get => _trainstationCode; set => _trainstationCode = value; }

    private string _trainstationName = null!;
    private string TrainstationName { get => _trainstationName; set => _trainstationName = value; }

    private int? _platform;
    private int? Platform { get => _platform; set => _platform = value; }

    private int? _trainSize;
    private int? TrainSize { get => _trainSize; set => _trainSize = value; }
}
