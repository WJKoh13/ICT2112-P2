using Microsoft.Extensions.DependencyInjection;
using ProRental.Data.Module3.P2_1;
using ProRental.Domain.Controls;
using ProRental.Interfaces.Module3.P2_1;

namespace ProRental.Configuration.Module3.P2_1;

/// <summary>
/// Registers the Feature 1 shipping-option stack and its temporary local adapters.
/// by: ernest
/// </summary>
public static class Feature1ServiceCollectionExtensions
{
    public static IServiceCollection AddFeature1Services(this IServiceCollection services)
    {
        services.AddScoped<IShippingOptionMapper, ShippingOptionMapper>();
        services.AddScoped<IOrderService, ShippingOrderContextService>();
        services.AddScoped<IRoutingService, ShippingRoutingService>();
        services.AddScoped<ITransportCarbonService, ShippingTransportCarbonService>();
        services.AddScoped<IShippingOptionService, ShippingOptionManager>();
        services.AddScoped<IRankingService, RankingManager>();
        services.AddScoped<IRankingStrategy, FastestStrategy>();
        services.AddScoped<IRankingStrategy, CheapestStrategy>();
        services.AddScoped<IRankingStrategy, EcoFriendlyStrategy>();

        return services;
    }
}
