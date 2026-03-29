using ProRental.Models.Module3.P2_1;

namespace ProRental.Interfaces.Module3.P2_1;

/// <summary>
/// Module 1 integration contract consumed by Module 3 shipping workflows that
/// still need order-backed delivery inputs.
/// by: ernest
/// </summary>
public interface IOrderService
{
    Task<OrderShippingContext?> GetShippingContextAsync(
        int orderId,
        CancellationToken cancellationToken = default);
}
