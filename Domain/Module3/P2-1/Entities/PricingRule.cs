using ProRental.Domain.Enums;

namespace ProRental.Domain.Entities;

public partial class PricingRule
{
    private TransportMode? _transportMode;
    private TransportMode? TransportMode { get => _transportMode; set => _transportMode = value; }

    public int ReadRuleId() => getRuleId();

    public TransportMode? ReadTransportMode() => getTransportMode();

    public decimal? ReadBaseRatePerKm() => getBaseRatePerKm();

    public bool ReadIsActive() => getIsActive();

    public decimal? ReadCarbonSurcharge() => getCarbonSurcharge();
}
