using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class ProductReturn
{
    private int _returnId;
    public int ReturnId { get => _returnId; set => _returnId = value; }

    private string? _returnStatus;
    public string? ReturnStatus { get => _returnStatus; set => _returnStatus = value; }

    private double? _totalCarbon;
    public double? TotalCarbon { get => _totalCarbon; set => _totalCarbon = value; }

    private DateOnly? _dateIn;
    public DateOnly? DateIn { get => _dateIn; set => _dateIn = value; }

    private DateOnly? _dateOn;
    public DateOnly? DateOn { get => _dateOn; set => _dateOn = value; }
}
