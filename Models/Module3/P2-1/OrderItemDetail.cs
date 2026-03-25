namespace ProRental.Models.Module3.P2_1;

public sealed record OrderItemDetail(
    int OrderItemId,
    int ProductId,
    int Quantity,
    double UnitPrice,
    double WeightKg);
