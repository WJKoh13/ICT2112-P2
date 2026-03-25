using Microsoft.EntityFrameworkCore;
using ProRental.Data.UnitOfWork;
using ProRental.Domain.Entities;
using ProRental.Interfaces.Module3.P2_1;

namespace ProRental.Domain.Module3.P2_1.Controls;

public sealed class RouteQueryService : IRouteQueryService
{
    private readonly AppDbContext _context;

    public RouteQueryService(AppDbContext context)
    {
        _context = context;
    }

    public Task<RouteLeg?> RetrieveFirstMileLeg(int routeId, CancellationToken cancellationToken = default)
    {
        return _context.RouteLegs
            .AsNoTracking()
            .Where(item =>
                EF.Property<int>(item, "RouteId") == routeId &&
                EF.Property<bool?>(item, "IsFirstMile") == true)
            .OrderBy(item => EF.Property<int?>(item, "Sequence"))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
