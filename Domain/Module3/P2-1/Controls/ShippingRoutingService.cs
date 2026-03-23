using ProRental.Data.UnitOfWork;
using ProRental.Domain.Enums;
using ProRental.Interfaces.Module3.P2_1;
using ProRental.Models.Module3.P2_1;

namespace ProRental.Domain.Controls;

/// <summary>
/// Temporary Feature 3 adapter used to keep Feature 1 runnable. It preserves the
/// DeliveryRoute-based dependency, but the route body is synthesized locally until
/// the routing subsystem exposes its intended multi-leg implementation.
/// by: ernest
/// </summary>
public sealed class ShippingRoutingService : IRoutingService
{
    private readonly AppDbContext _context;

    public ShippingRoutingService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Domain.Entities.DeliveryRoute> CreateRouteAsync(
        RoutingRequest request,
        CancellationToken cancellationToken = default)
    {
        var route = new Domain.Entities.DeliveryRoute();
        // This is a stub route persisted only so Feature 1 can reference a real route row
        // during the prototype. It does not yet model hubs, legs, or mapping-provider output.
        route.SetOriginAddress("ProRental Warehouse");
        route.SetDestinationAddress(request.DestinationAddress);
        route.SetTotalDistanceKm(CalculateDistanceKm(request.PreferenceType, request.WeightKg, request.Quantity));
        route.SetIsValid(true);

        await _context.DeliveryRoutes.AddAsync(route, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return route;
    }

    private static double CalculateDistanceKm(PreferenceType preferenceType, double weightKg, int quantity)
    {
        // Deterministic placeholder distances keep checkout behavior and tests stable
        // until Feature 3 supplies actual multi-modal route planning.
        var baseDistance = preferenceType switch
        {
            PreferenceType.FAST => 18d,
            PreferenceType.CHEAP => 42d,
            _ => 26d
        };

        var loadOffset = Math.Max(weightKg - 1d, 0d) + Math.Max(quantity - 1, 0);
        return Math.Round(baseDistance + loadOffset, 2, MidpointRounding.AwayFromZero);
    }
}
