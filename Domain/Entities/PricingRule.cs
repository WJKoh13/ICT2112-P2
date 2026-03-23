using System;
using System.Collections.Generic;

namespace ProRental.Domain.Entities;

public partial class PricingRule
{
    private int _ruleId;
    public int RuleId { get => _ruleId; set => _ruleId = value; }

    private decimal? _baseRatePerKm;
    public decimal? BaseRatePerKm { get => _baseRatePerKm; set => _baseRatePerKm = value; }

    private bool? _isActive;
    public bool? IsActive { get => _isActive; set => _isActive = value; }

    private decimal? _carbonSurcharge;
    public decimal? CarbonSurcharge { get => _carbonSurcharge; set => _carbonSurcharge = value; }
}
