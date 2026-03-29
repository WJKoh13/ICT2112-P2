using Microsoft.Extensions.DependencyInjection;
using ProRental.Data.Mappers;
using ProRental.Data.Module3.P2_1;
using ProRental.Data.Module3.P2_1.Gateways;
using ProRental.Data.Interfaces;
using ProRental.Data.Module3.P2_1.Interfaces;
using ProRental.Data.Module3.P2_1.Mappers;
using ProRental.Data.Module3.P2_1.Services;
using ProRental.Domain.Control;
using ProRental.Domain.Controls;
using ProRental.Domain.Module2.P2_3.Controls;
using ProRental.Domain.Module3.P2_1.Controls;
using ProRental.Interfaces;
using ProRental.Interfaces.Module2.P2_3;
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
        services.AddHttpClient<IGoogleMapsAPI, GoogleMapsAPI>(client =>
        {
            client.BaseAddress = new Uri("https://routes.googleapis.com/");
        });
        services.AddScoped<IRouteMapper, RouteMapper>();
        services.AddScoped<ITransportationHubMapper, TransportationHubMapper>();
        services.AddScoped<IProductQuery, ProductQuery>();
        services.AddScoped<IShippingOptionMapper, ShippingOptionMapper>();
        services.AddScoped<ICheckoutShippingContextService, ShippingCheckoutContextService>();
        services.AddScoped<IRouteDistanceCalculator, RouteDistanceCalculator>();
        services.AddScoped<IRouteQueryService, RouteManager>();
        services.AddScoped<IRouteLegBuilder, RouteLegBuilder>();
        services.AddScoped<IRoutingService, RouteManager>();
        services.AddScoped<IPricingRuleGateway, PricingRuleGateway>();
        services.AddScoped<ITransportCarbonService, ProRental.Domain.Module3.P2_1.Controls.TransportCarbonManager>();
        services.AddScoped<IShippingPreferenceStrategy, FastShippingPreferenceStrategy>();
        services.AddScoped<IShippingPreferenceStrategy, CheapShippingPreferenceStrategy>();
        services.AddScoped<IShippingPreferenceStrategy, GreenShippingPreferenceStrategy>();
        services.AddScoped<IShippingPreferenceService, PreferenceManager>();
        services.AddScoped<IShippingOptionService, ShippingOptionManager>();

        return services;
    }
}
