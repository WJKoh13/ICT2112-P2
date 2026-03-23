// Data/Module3/Gateways/OrderCarbonDataGateway.cs
// Concrete Table Data Gateway for the ordercarbondatum table.

using ProRental.Data.UnitOfWork;
using ProRental.Domain.Entities;
using ProRental.Interfaces.Data;

namespace ProRental.Data;

public class OrderCarbonDataGateway : IOrderCarbonDataGateway
{
    private readonly AppDbContext _db;

    public OrderCarbonDataGateway(AppDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc/>
    public void Save(Ordercarbondatum data)
    {
        _db.Ordercarbondata.Add(data);
        _db.SaveChanges();
    }

    /// <inheritdoc/>
    public Ordercarbondatum? FindByOrderId(int orderId)
        => _db.Ordercarbondata
              .FirstOrDefault(d => d.Orderid == orderId);

    /// <inheritdoc/>
    public List<Ordercarbondatum> FindAll()
        => _db.Ordercarbondata
              .OrderByDescending(d => d.Calculatedat)
              .ToList();
}
