// Domain/Module3/P2-5/RewardsControl.cs
// Business logic for the Rewards Dashboard feature (Feature 4 — P2-5).
// Depends only on gateway interfaces — never on concrete gateway classes.

using ProRental.Domain.Entities;
using ProRental.Interfaces.Data;
using ProRental.Interfaces.Domain;

namespace ProRental.Domain.Controls;

public class RewardsControl : IRewardsControl
{
    // ── Constants ─────────────────────────────────────────────────────────────
    // Carbon thresholds (grams CO₂e) used to determine eco-score and rewards.
    private const double LowCarbonThreshold    = 500.0;   // score ≥ 80 → Gold reward
    private const double MediumCarbonThreshold = 1500.0;  // score ≥ 50 → Silver reward
    // Above MediumCarbonThreshold                         // score < 50 → no reward

    private const double GoldDiscountValue   = 15.0;  // 15% discount
    private const double SilverDiscountValue = 8.0;   // 8% discount

    // ── Dependencies ──────────────────────────────────────────────────────────
    private readonly IOrderCarbonDataGateway _carbonGateway;
    private readonly IRewardGateway          _rewardGateway;

    public RewardsControl(
        IOrderCarbonDataGateway carbonGateway,
        IRewardGateway          rewardGateway)
    {
        _carbonGateway = carbonGateway;
        _rewardGateway = rewardGateway;
    }

    // ── IRewardsControl ───────────────────────────────────────────────────────

    /// <inheritdoc/>
    public Ordercarbondatum CreateOrderCarbonData(int orderId, double totalCarbon)
    {
        var existing = _carbonGateway.FindByOrderId(orderId);
        if (existing is not null)
            return existing; // idempotent — don't duplicate

        var record = new Ordercarbondatum
        {
            Orderid      = orderId,
            Totalcarbon  = totalCarbon,
            Calculatedat = DateTime.UtcNow,
            Impactlevel  = DeriveImpactLevel(totalCarbon)
        };

        _carbonGateway.Save(record);
        return record;
    }

    /// <inheritdoc/>
    public int CalculateEcoScore(int orderId)
    {
        var data = _carbonGateway.FindByOrderId(orderId);
        if (data is null) return -1;

        // Linear scale: 0 g CO₂e → 100 points, 3000 g CO₂e → 0 points.
        const double maxCarbon = 3000.0;
        var score = (int)Math.Round(
            Math.Max(0, (1.0 - data.Totalcarbon / maxCarbon) * 100)
        );
        return Math.Clamp(score, 0, 100);
    }

    /// <inheritdoc/>
    public Customerreward? DetermineReward(int orderId, int customerId)
    {
        var score = CalculateEcoScore(orderId);
        if (score < 0) return null; // no carbon data yet

        var data = _carbonGateway.FindByOrderId(orderId)!;

        string?  rewardType  = null;
        double   rewardValue = 0;

        if (data.Totalcarbon <= LowCarbonThreshold)
        {
            rewardType  = "Gold Discount";
            rewardValue = GoldDiscountValue;
        }
        else if (data.Totalcarbon <= MediumCarbonThreshold)
        {
            rewardType  = "Silver Discount";
            rewardValue = SilverDiscountValue;
        }
        else
        {
            return null; // does not qualify
        }

        var reward = new Customerreward
        {
            Customerid        = customerId,
            Ordercarbondataid = data.Ordercarbondataid,
            Rewardtype        = rewardType,
            Rewardvalue       = rewardValue,
            Createdat         = DateTime.UtcNow
        };

        _rewardGateway.Save(reward);
        return reward;
    }

    /// <inheritdoc/>
    public List<Customerreward> GetRewardsForCustomer(int customerId)
        => _rewardGateway.FindAllByCustomerId(customerId);

    /// <inheritdoc/>
    public List<Ordercarbondatum> GetAllOrderCarbonData()
        => _carbonGateway.FindAll();

    // ── Private helpers ───────────────────────────────────────────────────────

    private static string DeriveImpactLevel(double totalCarbon) => totalCarbon switch
    {
        <= 500  => "Low",
        <= 1500 => "Medium",
        _       => "High"
    };
}
