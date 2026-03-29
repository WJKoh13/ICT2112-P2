using Microsoft.Extensions.DependencyInjection;
using ProRental.Data.Mappers;
using ProRental.Data.Module3.P2_1;
using ProRental.Data.Module3.P2_1.Gateways;
using ProRental.Data.Interfaces;
using ProRental.Data.Module3.P2_1.Interfaces;
using ProRental.Domain.Control;
using ProRental.Domain.Controls;
using ProRental.Domain.Module3.P2_1.Controls;
using ProRental.Interfaces;
using ProRental.Interfaces.Module3.P2_1;

namespace ProRental.Configuration.Module3.P2_1;

/// <summary>
/// Registers the Feature 1 shipping-option stack and its cross-feature adapters.
/// by: ernest
/// </summary>
public static class Feature1ServiceCollectionExtensions
{
    public static IServiceCollection AddFeature1Services(this IServiceCollection services)
    {
        services.AddHttpClient<IGoogleMapsApi, GoogleMapsAPI>(client =>
        {
            client.BaseAddress = new Uri("https://routes.googleapis.com/");
        });
        services.AddScoped<ITransportationHubMapper, TransportationHubMapper>();
        services.AddScoped<IInventoryService, DummyInventoryService>();
        services.AddScoped<IShippingOptionMapper, ShippingOptionMapper>();
        services.AddScoped<IOrderService, ShippingOrderContextService>();
        services.AddScoped<IRouteDistanceCalculator, RouteDistanceCalculator>();
        services.AddScoped<IRouteQueryService, RouteManager>();
        services.AddScoped<IRouteLegBuilder, RouteLegBuilder>();
        services.AddScoped<IRoutingService, RouteManager>();
        services.AddScoped<IPricingRuleGateway, PricingRuleGateway>();
        services.AddScoped<ITransportCarbonService, ProRental.Domain.Module3.P2_1.Controls.TransportCarbonManager>();
        services.AddScoped<IShippingOptionService, ShippingOptionManager>();
        services.AddScoped<IRankingService, RankingManager>();
        services.AddScoped<IRankingStrategy, FastestStrategy>();
        services.AddScoped<IRankingStrategy, CheapestStrategy>();
        services.AddScoped<IRankingStrategy, EcoFriendlyStrategy>();

        return services;
    }
}
