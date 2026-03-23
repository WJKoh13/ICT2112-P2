// Data/Module3/Gateways/RewardGateway.cs
// Concrete Table Data Gateway for the customerreward table.
// Uses AppDbContext (Unit of Work) injected via constructor.

using ProRental.Data.UnitOfWork;
using ProRental.Domain.Entities;
using ProRental.Interfaces.Data;

namespace ProRental.Data;

public class RewardGateway : IRewardGateway
{
    private readonly AppDbContext _db;

    public RewardGateway(AppDbContext db)
    {
        _db = db;
    }

    /// <inheritdoc/>
    public void Save(Customerreward reward)
    {
        _db.Customerrewards.Add(reward);
        _db.SaveChanges();
    }

    /// <inheritdoc/>
    public Customerreward? FindByCustomerId(int customerId)
        => _db.Customerrewards
              .Where(r => r.Customerid == customerId)
              .OrderByDescending(r => r.Createdat)
              .FirstOrDefault();

    /// <inheritdoc/>
    public List<Customerreward> FindAllByCustomerId(int customerId)
        => _db.Customerrewards
              .Where(r => r.Customerid == customerId)
              .OrderByDescending(r => r.Createdat)
              .ToList();

    /// <inheritdoc/>
    public void UpdateRewardValue(int customerId, double newValue)
    {
        var reward = FindByCustomerId(customerId);
        if (reward is null) return;

        reward.Rewardvalue = newValue;
        _db.SaveChanges();
    }
}
