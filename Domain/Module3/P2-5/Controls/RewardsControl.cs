using ProRental.Domain.Entities;
using ProRental.Interfaces.Data;
using ProRental.Interfaces.Domain;

namespace ProRental.Domain.Controls;

public class RewardsControl : IRewardsControl
{
    // Carbon thresholds (grams CO₂e)
    private const double LowThreshold      = 50.0;
    private const double ModerateThreshold = 100.0;
    private const double HighThreshold     = 200.0;

    private readonly IOrderCarbonDataGateway  _carbonGateway;
    private readonly IRewardGateway           _rewardGateway;
    private readonly IProductFootprintService _productFootprintService;

    public RewardsControl(
        IOrderCarbonDataGateway  carbonGateway,
        IRewardGateway           rewardGateway,
        IProductFootprintService productFootprintService)
    {
        _carbonGateway           = carbonGateway;
        _rewardGateway           = rewardGateway;
        _productFootprintService = productFootprintService;
    }

    /// <summary>
    /// Aggregates carbon from all four sources, classifies impact level,
    /// and persists the result. Idempotent — returns existing record if
    /// already calculated for this order.
    /// </summary>
    public Ordercarbondatum CreateOrderCarbonData(int orderId, double totalCarbon)
    {
        var existing = _carbonGateway.FindByOrderId(orderId);
        if (existing is not null) return existing;

        // Retrieve each footprint component.
        // Product carbon comes from Feature 1 via IProductFootprintService.
        // Packaging, staff, building stubs return 0 until those features integrate.
        double productCarbon    = _productFootprintService.CalculateProductFootprint(orderId);
        double packagingCarbon  = 0.0; // Feature 3 integration point
        double staffCarbon      = 0.0; // Feature 1 organisational integration point
        double buildingCarbon   = 0.0; // Feature 1 organisational integration point

        double total = productCarbon + packagingCarbon + staffCarbon + buildingCarbon;

        // Allow caller to pass in a total override (e.g. from manual entry in UI)
        // If totalCarbon > 0 was passed, use that instead.
        if (totalCarbon > 0) total = totalCarbon;

        var record = new Ordercarbondatum
        {
            Orderid         = orderId,
            Productcarbon   = productCarbon,
            Packagingcarbon = packagingCarbon,
            Staffcarbon     = staffCarbon,
            Buildingcarbon  = buildingCarbon,
            Totalcarbon     = total,
            Impactlevel     = ClassifyImpact(total),
            Calculatedat    = DateTime.UtcNow
        };

        _carbonGateway.Save(record);
        return record;
    }

    /// <summary>
    /// Calculates an eco-score (0–100) based on total carbon.
    /// Returns -1 if no carbon data exists for the order.
    /// </summary>
    public int CalculateEcoScore(int orderId)
    {
        var data = _carbonGateway.FindByOrderId(orderId);
        if (data is null) return -1;

        // Linear scale: 0 g → 100 pts, 400 g → 0 pts
        const double maxCarbon = 400.0;
        int score = (int)Math.Round(Math.Max(0, (1.0 - data.Totalcarbon / maxCarbon) * 100));
        return Math.Clamp(score, 0, 100);
    }

    /// <summary>
    /// Determines and persists a reward based on the order's impact level.
    /// Returns null if the order does not qualify.
    /// </summary>
    public Customerreward? DetermineReward(int orderId)
    {
        var data = _carbonGateway.FindByOrderId(orderId);
        if (data is null) return null;

        // Check if reward already issued for this carbon record
        var existing = _rewardGateway.FindByOrderCarbonDataId(data.Ordercarbondataid);
        if (existing is not null) return existing;

        (string type, double value)? rewardInfo = data.Impactlevel switch
        {
            "Low"      => ("Voucher", 10.0),
            "Moderate" => ("Voucher", 5.0),
            _          => null
        };

        if (rewardInfo is null) return null;

        var reward = new Customerreward
        {
            Customerid        = 1, // placeholder until auth is integrated
            Ordercarbondataid = data.Ordercarbondataid,
            Rewardtype        = rewardInfo.Value.type,
            Rewardvalue       = rewardInfo.Value.value,
            Createdat         = DateTime.UtcNow
        };

        _rewardGateway.Save(reward);
        return reward;
    }

    public List<Ordercarbondatum> GetAllCarbonRecords()
        => _carbonGateway.FindAll();

    public List<Customerreward> GetAllRewards()
        => _rewardGateway.FindAll();

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string ClassifyImpact(double totalCarbon) => totalCarbon switch
    {
        < LowThreshold      => "Low",
        < ModerateThreshold => "Moderate",
        < HighThreshold     => "High",
        _                   => "Very High"
    };
}