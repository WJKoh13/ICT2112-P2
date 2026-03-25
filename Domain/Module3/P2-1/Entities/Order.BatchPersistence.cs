using ProRental.Domain.Enums;

namespace ProRental.Domain.Entities;

public partial class Order
{
    public int GetOrderId() => _orderid;
    public OrderStatus? GetStatus() => _status;
}
