// Interfaces/Module3/IRewardsControl.cs
// Domain-layer service interface for the Rewards Dashboard feature.
// Controllers depend on this interface — never on RewardsControl directly.

using ProRental.Domain.Entities;

namespace ProRental.Interfaces.Domain;

public interface IRewardsControl
{
    /// <summary>
    /// Creates and persists an OrderCarbonData record linking an order to its
    /// total carbon footprint. Returns the saved entity.
    /// </summary>
    Ordercarbondatum CreateOrderCarbonData(int orderId, double totalCarbon);

    /// <summary>
    /// Calculates an eco-score (0–100) for the given order based on its
    /// recorded carbon footprint. Higher score = greener order.
    /// Returns -1 if no carbon data exists for the order.
    /// </summary>
    int CalculateEcoScore(int orderId);

    /// <summary>
    /// Determines the appropriate reward for a customer based on the order's
    /// eco-score and persists it. Returns the saved reward entity, or null
    /// if the order does not qualify.
    /// </summary>
    Customerreward? DetermineReward(int orderId, int customerId);

    /// <summary>Returns all rewards for the given customer (for dashboard display).</summary>
    List<Customerreward> GetRewardsForCustomer(int customerId);

    /// <summary>Returns all order carbon records (for dashboard table).</summary>
    List<Ordercarbondatum> GetAllOrderCarbonData();
}
