// Data/Module3/Interfaces/IRewardGateway.cs
// Table Data Gateway contract for the customerreward table.
// Consumed only by Domain/Control classes — never by Controllers directly.

using ProRental.Domain.Entities;

namespace ProRental.Interfaces.Data;

public interface IRewardGateway
{
    /// <summary>Persists a new reward record.</summary>
    void Save(Customerreward reward);

    /// <summary>Returns the most recent reward for a given customer, or null.</summary>
    Customerreward? FindByCustomerId(int customerId);

    /// <summary>Returns all rewards for a given customer.</summary>
    List<Customerreward> FindAllByCustomerId(int customerId);

    /// <summary>Updates the reward value (e.g. discount percentage) for a customer's latest reward.</summary>
    void UpdateRewardValue(int customerId, double newValue);
}
