// Data/Module3/Interfaces/IOrderCarbonDataGateway.cs
// Table Data Gateway contract for the ordercarbondatum table.
// Consumed only by Domain/Control classes — never by Controllers directly.

using ProRental.Domain.Entities;

namespace ProRental.Interfaces.Data;

public interface IOrderCarbonDataGateway
{
    /// <summary>Persists a new order carbon data record.</summary>
    void Save(Ordercarbondatum data);

    /// <summary>Returns carbon data for a specific order, or null if not yet calculated.</summary>
    Ordercarbondatum? FindByOrderId(int orderId);

    /// <summary>Returns all carbon data records (for dashboard listing).</summary>
    List<Ordercarbondatum> FindAll();
}
