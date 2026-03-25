using ProRental.Models.Module3.P2_1;

namespace ProRental.Interfaces.Module3.P2_1;

/// <summary>
/// Module 1 integration contract consumed by Feature 1 to obtain checkout inputs.
/// by: ernest
/// </summary>
public interface IOrderService
{
    Task<OrderShippingContext?> GetShippingContextAsync(
        int orderId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<OrderItemDetail>> OrderDetails(
        int orderId,
        CancellationToken cancellationToken = default);

    Task<int> GetOrderItemQuantity(
        int orderId,
        int productId,
        CancellationToken cancellationToken = default);

    Task<string?> GetDeliveryAddress(
        int orderId,
        CancellationToken cancellationToken = default);

    Task<bool> IsOrderDispatchedAsync(
        int orderId,
        CancellationToken cancellationToken = default);
}
