using ProRental.Data.Module3.P2_5.Interfaces;
using ProRental.Data.UnitOfWork;
using ProRental.Domain.Entities;
using ProRental.Domain.Module3.P2_5.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace ProRental.Data.Module3.P2_5.Gateways;

public sealed class StaffFootprintGateway : IStaffFootprintGateway
{
    private readonly AppDbContext _dbContext;

    public StaffFootprintGateway(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public List<ChartData> GetHourlyChartData()
    {
        return _dbContext.Stafffootprints
            .AsEnumerable()
            .GroupBy(GetTime)
            .Select(group => new ChartData(
                group.Key.ToString("yyyy-MM-dd HH:mm"),
                Math.Round(group.Sum(GetTotalStaffCo2), 2)))
            .OrderBy(chart => chart.Label)
            .ToList();
    }

    public List<ChartData> GetStaffGraphData()
    {
        return _dbContext.Stafffootprints
            .AsEnumerable()
            .GroupBy(GetStaffId)
            .Select(group => new ChartData(
                $"Staff {group.Key}",
                Math.Round(group.Sum(GetTotalStaffCo2), 2)))
            .OrderByDescending(graph => graph.Value)
            .ThenBy(graph => graph.Label)
            .ToList();
    }

    public List<ChartData> GetHotspotData(int top = 5)
    {
        return _dbContext.Stafffootprints
            .AsEnumerable()
            .GroupBy(GetStaffId)
            .Select(group => new ChartData(
                $"Staff {group.Key}",
                Math.Round(group.Average(GetTotalStaffCo2), 2)))
            .OrderByDescending(hotspot => hotspot.Value)
            .ThenBy(hotspot => hotspot.Label)
            .Take(top)
            .ToList();
    }

    public Task<bool> StaffExistsAsync(int staffId)
    {
        return _dbContext.Staff.AnyAsync(staff => EF.Property<int>(staff, "Staffid") == staffId);
    }

    public Task<string?> GetDepartmentByStaffIdAsync(int staffId)
    {
        return _dbContext.Staff
            .Where(staff => EF.Property<int>(staff, "Staffid") == staffId)
            .Select(staff => EF.Property<string>(staff, "Department"))
            .FirstOrDefaultAsync();
    }

    public Task<List<StaffLookupItem>> GetStaffLookupAsync()
    {
        var items = _dbContext.Staff
            .AsEnumerable()
            .Select(staff => new StaffLookupItem(
                ReadMember<int>(staff, "Staffid", "_staffid"),
                ReadMember<string>(staff, "Department", "_department")))
            .OrderBy(item => item.StaffId)
            .ToList();

        return Task.FromResult(items);
    }

    public async Task<Stafffootprint> CreateStaffFootprintAsync(int staffId, DateTime time, double hoursWorked, double totalStaffCo2)
    {
        var footprint = new Stafffootprint();
        WriteMember(footprint, "Staffid", "_staffid", staffId);
        WriteMember(footprint, "Time", "_time", time);
        WriteMember(footprint, "Hoursworked", "_hoursworked", hoursWorked);
        WriteMember(footprint, "Totalstaffco2", "_totalstaffco2", totalStaffCo2);

        _dbContext.Stafffootprints.Add(footprint);
        await _dbContext.SaveChangesAsync();
        return footprint;
    }

    private static DateTime GetTime(Stafffootprint footprint)
    {
        return ReadMember<DateTime>(footprint, "Time", "_time");
    }

    private static int GetStaffId(Stafffootprint footprint)
    {
        return ReadMember<int>(footprint, "Staffid", "_staffid");
    }

    private static double GetTotalStaffCo2(Stafffootprint footprint)
    {
        return ReadMember<double>(footprint, "Totalstaffco2", "_totalstaffco2");
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
