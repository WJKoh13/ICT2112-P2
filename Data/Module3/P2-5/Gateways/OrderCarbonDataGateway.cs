using ProRental.Data.UnitOfWork;
using ProRental.Domain.Entities;
using ProRental.Interfaces.Data;
using System.Reflection;

namespace ProRental.Data;

public class OrderCarbonDataGateway : IOrderCarbonDataGateway
{
    private readonly AppDbContext _db;

    public OrderCarbonDataGateway(AppDbContext db)
    {
        _db = db;
    }

    public void Save(Ordercarbondatum data)
    {
        WriteMember(data, "Calculatedat", "_calculatedat", NormalizeTimestamp(ReadMember<DateTime>(data, "Calculatedat", "_calculatedat")));
        _db.Ordercarbondata.Add(data);
        _db.SaveChanges();
    }

    public Ordercarbondatum? FindByOrderId(int orderId)
        => _db.Ordercarbondata.ToList().FirstOrDefault(d => d.GetOrderid() == orderId);

    public List<Ordercarbondatum> FindAll()
        => _db.Ordercarbondata
              .ToList()
              .OrderByDescending(d => d.GetCalculatedat())
              .ToList();

    private static DateTime NormalizeTimestamp(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }

    private static T ReadMember<T>(object source, string propertyName, string fieldName)
    {
        var type = source.GetType();

        var property = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (property?.GetValue(source) is T propertyValue)
        {
            return propertyValue;
        }

        var field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (field?.GetValue(source) is T fieldValue)
        {
            return fieldValue;
        }

        throw new InvalidOperationException($"Unable to read '{propertyName}' from {type.Name}.");
    }

    private static void WriteMember<T>(object target, string propertyName, string fieldName, T value)
    {
        var type = target.GetType();

        var property = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (property != null)
        {
            property.SetValue(target, value);
            return;
        }

        var field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (field != null)
        {
            field.SetValue(target, value);
            return;
        }

        throw new InvalidOperationException($"Unable to write '{propertyName}' on {type.Name}.");
    }
}
