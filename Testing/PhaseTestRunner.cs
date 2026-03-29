using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using ProRental.Configuration.Module3.P2_1;
using ProRental.Controllers;
using ProRental.Data.Mappers;
using ProRental.Data.Interfaces;
using ProRental.Data.UnitOfWork;
using ProRental.Data.Module3.P2_1;
using ProRental.Data.Module3.P2_1.Gateways;
using ProRental.Data.Module3.P2_1.Interfaces;
using ProRental.Data.Module3.P2_1.Mappers;
using ProRental.Data.Module3.P2_1.Services;
using ProRental.Domain.Controls;
using ProRental.Domain.Module3.P2_1.Controls;
using ProRental.Domain.Entities;
using ProRental.Domain.Enums;
using ProRental.Interfaces;
using ProRental.Interfaces.Module2.P2_3;
using ProRental.Interfaces.Module3.P2_1;
using ProRental.Models.Module3.P2_1;
using System.Reflection;

namespace ProRental.Testing;

internal static class PhaseTestRunner
{
    public static Task<int> RunAsync(string[] args)
    {
        var phase = (args.FirstOrDefault() ?? "phase0").Trim().ToLowerInvariant();

        var tests = phase switch
        {
            "transportcarbon" => TransportCarbonManagerTests.All,
            "phase0" => Phase0Tests.All,
            "phase1" => Phase1Tests.All,
            "phase2" => Phase2Tests.All,
            "phase3" => Phase3Tests.All,
            "phase4" => Phase4Tests.All,
            "phase5" => Phase5Tests.All,
            "phase6" => Phase6Tests.All,
            "phase7" => Phase7Tests.All,
            _ => throw new InvalidOperationException($"Unknown phase '{phase}'.")
        };

        var failures = new List<string>();

        foreach (var test in tests)
        {
            try
            {
                test.Action();
                Console.WriteLine($"PASS {test.Name}");
            }
            catch (Exception ex)
            {
                failures.Add($"{test.Name}: {FormatException(ex)}");
                Console.WriteLine($"FAIL {test.Name}");
            }
        }

        if (failures.Count == 0)
        {
            Console.WriteLine($"All {tests.Count} tests passed for {phase}.");
            return Task.FromResult(0);
        }

        Console.WriteLine($"Phase {phase} failed with {failures.Count} test failure(s):");
        foreach (var failure in failures)
        {
            Console.WriteLine(failure);
        }

        return Task.FromResult(1);
    }

    private static string FormatException(Exception exception)
    {
        var parts = new List<string>();
        Exception? current = exception;

        while (current is not null)
        {
            parts.Add(current.Message);
            current = current.InnerException;
        }

        return string.Join(" | ", parts);
    }
}

internal sealed record PhaseTest(string Name, Action Action);

internal sealed class StubHubCarbonService : IHubCarbonService
{
    public double CalculateHubCarbon(int hubId, double hours) => 0d;
    public double CalculateProductStorageCarbon(int productId, int hubId) => 10d;
    public List<ItemCarbonInfo> GetProductItemCarbonBreakdown(int productId, int hubId) => [];
    public List<ItemCarbonInfo> RecommendItemsToClear(int hubId) => [];
    public List<ProductTimeInfo> GetProductTimeInWarehouse(int hubId) => [];
    public List<ProductStorageInfo> GetAllProductStorageInfo() => [];
}

// Phase 0 locks down the grouped entity accessors and EF mappings that Feature 1 relies on.
internal static class Phase0Tests
{
    public static IReadOnlyList<PhaseTest> All { get; } =
    [
        new("ShippingOption grouped methods round-trip values", ShippingOptionAccessorsRoundTrip),
        new("Checkout grouped methods round-trip values", CheckoutSelectionAccessorsRoundTrip),
        new("Order grouped methods round-trip values", OrderAccessorsRoundTrip),
        new("DeliveryRoute accessors round-trip values", DeliveryRouteAccessorsRoundTrip),
        new("Feature1 enum mappings use snake_case column names", Feature1EnumMappingsUseSnakeCase)
    ];

    private static void ShippingOptionAccessorsRoundTrip()
    {
        var option = new ShippingOption();
        option.ConfigureGeneratedOption(11, 22, PreferenceType.GREEN, "Green Express", 42.50m, 12.75, 4, TransportMode.TRAIN);

        var summary = option.GetSummary();
        var selection = option.GetSelectionResult();

        TestAssertions.AssertEqual("Green Express", summary.DisplayName);
        TestAssertions.AssertEqual(42.50m, summary.Cost);
        TestAssertions.AssertEqual(12.75, summary.CarbonFootprintKg);
        TestAssertions.AssertEqual(4, summary.DeliveryDays);
        TestAssertions.AssertEqual(11, summary.CheckoutId);
        TestAssertions.AssertEqual(22, summary.RouteId);
        TestAssertions.AssertEqual(PreferenceType.GREEN, summary.PreferenceType);
        TestAssertions.AssertEqual(TransportMode.TRAIN, summary.TransportMode);
        TestAssertions.AssertEqual(summary.OptionId, selection.OptionId);
    }

    private static void CheckoutSelectionAccessorsRoundTrip()
    {
        var checkout = new Checkout();
        var createdAt = new DateTime(2026, 03, 22, 10, 30, 00, DateTimeKind.Utc);

        checkout.Initialize(2, 5, createdAt);
        checkout.SelectShippingOption(9);

        var context = checkout.GetCheckoutContext();
        var selection = checkout.GetSelectionState();

        TestAssertions.AssertEqual(2, context.CustomerId);
        TestAssertions.AssertEqual(5, context.CartId);
        TestAssertions.AssertEqual(createdAt, context.CreatedAt);
        TestAssertions.AssertEqual(9, selection.SelectedOptionId);

        checkout.ClearSelectedShippingOption();

        TestAssertions.AssertNull(checkout.GetSelectionState().SelectedOptionId);
    }

    private static void OrderAccessorsRoundTrip()
    {
        var order = new Order();
        var orderDate = new DateTime(2026, 03, 22, 12, 00, 00, DateTimeKind.Utc);

        order.InitializeForCheckout(3, 7, orderDate, 199.99m, 8);

        var context = order.GetOrderContext();
        var snapshot = order.GetOrderSnapshot();

        TestAssertions.AssertEqual(3, context.CustomerId);
        TestAssertions.AssertEqual(7, context.CheckoutId);
        TestAssertions.AssertEqual(8, snapshot.TransactionId);
        TestAssertions.AssertEqual(orderDate, snapshot.OrderDate);
        TestAssertions.AssertEqual(199.99m, snapshot.TotalAmount);
    }

    private static void DeliveryRouteAccessorsRoundTrip()
    {
        var route = new DeliveryRoute();

        route.SetOriginAddress("Warehouse A");
        route.SetDestinationAddress("Customer B");
        route.SetTotalDistanceKm(128.5);
        route.SetIsValid(true);

        TestAssertions.AssertEqual("Warehouse A", route.GetOriginAddress());
        TestAssertions.AssertEqual("Customer B", route.GetDestinationAddress());
        TestAssertions.AssertEqual(128.5, route.GetTotalDistanceKm());
        TestAssertions.AssertEqual(true, route.GetIsValid());
    }

    private static void Feature1EnumMappingsUseSnakeCase()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=testrental;Username=test;Password=test")
            .Options;

        using var context = new AppDbContext(options);
        var shippingOption = context.Model.FindEntityType(typeof(ShippingOption))
            ?? throw new InvalidOperationException("ShippingOption entity metadata was not found.");

        var shippingTable = StoreObjectIdentifier.Table("shipping_option", null);

        TestAssertions.AssertEqual(
            "preference_type",
            shippingOption.FindProperty("PreferenceType")?.GetColumnName(shippingTable));
        TestAssertions.AssertEqual(
            "transport_mode",
            shippingOption.FindProperty("TransportMode")?.GetColumnName(shippingTable));
    }

}

// Phase 1 verifies that the public contracts exposed by Feature 1 match the intended service boundaries.
internal static class Phase1Tests
{
    public static IReadOnlyList<PhaseTest> All { get; } =
    [
        new("Shipping option summary carries the full checkout payload", ShippingOptionSummaryCarriesFullCheckoutPayload),
        new("Feature1 service contract exposes the expected async operations", ShippingOptionServiceContractIsStable),
        new("Shipping preference strategy contract exposes the expected algorithm boundary", ShippingPreferenceStrategyContractIsStable),
        new("Shipping preference service contract exposes the expected context operations", ShippingPreferenceServiceContractIsStable),
        new("Repository and dependency contracts expose the next-phase entry points", RepositoryAndDependencyContractsArePresent)
    ];

    private static void ShippingOptionSummaryCarriesFullCheckoutPayload()
    {
        var summary = new ShippingOptionSummary(
            OptionId: 1,
            CheckoutId: 2,
            PreferenceType: PreferenceType.FAST,
            DisplayName: "Fastest",
            Cost: 25.00m,
            CarbonFootprintKg: 14.2,
            DeliveryDays: 2,
            RouteId: 8,
            TransportMode: TransportMode.PLANE,
            TransportModeLabel: "Plane + Truck");

        TestAssertions.AssertEqual(1, summary.OptionId);
        TestAssertions.AssertEqual(2, summary.CheckoutId);
        TestAssertions.AssertEqual(PreferenceType.FAST, summary.PreferenceType);
        TestAssertions.AssertEqual("Fastest", summary.DisplayName);
        TestAssertions.AssertEqual(25.00m, summary.Cost);
        TestAssertions.AssertEqual(14.2, summary.CarbonFootprintKg);
        TestAssertions.AssertEqual(2, summary.DeliveryDays);
        TestAssertions.AssertEqual(8, summary.RouteId);
        TestAssertions.AssertEqual(TransportMode.PLANE, summary.TransportMode);
        TestAssertions.AssertEqual("Plane + Truck", summary.TransportModeLabel);
    }

    private static void ShippingOptionServiceContractIsStable()
    {
        var contract = typeof(IShippingOptionService);

        AssertMethod(contract, "GetPreferenceChoicesForCheckoutAsync", typeof(Task<IReadOnlyList<ShippingPreferenceCard>>), typeof(int), typeof(CancellationToken));
        AssertMethod(contract, "ConfirmPreferenceSelectionAsync", typeof(Task<ShippingSelectionResult>), typeof(SelectShippingPreferenceRequest), typeof(CancellationToken));
    }

    private static void ShippingPreferenceStrategyContractIsStable()
    {
        var strategyContract = typeof(IShippingPreferenceStrategy);
        var preferenceProperty = strategyContract.GetProperty(nameof(IShippingPreferenceStrategy.PreferenceType))
            ?? throw new InvalidOperationException("IShippingPreferenceStrategy.PreferenceType was not found.");
        TestAssertions.AssertEqual(typeof(PreferenceType), preferenceProperty.PropertyType);
        AssertMethod(strategyContract, "ResolveAllowedModes", typeof(IReadOnlyList<TransportMode>), typeof(bool));
    }

    private static void ShippingPreferenceServiceContractIsStable()
    {
        var contract = typeof(IShippingPreferenceService);

        AssertMethod(contract, "BuildPreferenceCards", typeof(IReadOnlyList<ShippingPreferenceCard>), typeof(CheckoutShippingContext), typeof(bool));
        AssertMethod(contract, "ResolvePreference", typeof(ResolvedShippingPreference), typeof(PreferenceType), typeof(bool));
    }

    private static void RepositoryAndDependencyContractsArePresent()
    {
        var mapper = typeof(IShippingOptionMapper);
        var checkoutShippingContextService = typeof(ICheckoutShippingContextService);
        var routingService = typeof(IRoutingService);
        var transportCarbonService = typeof(ITransportCarbonService);
        var checkoutShippingContext = typeof(CheckoutShippingContext);

        AssertMethod(mapper, "FindCheckoutAsync", typeof(Task<Checkout?>), typeof(int), typeof(CancellationToken));
        AssertMethod(mapper, "FindByCheckoutIdAsync", typeof(Task<IReadOnlyList<ShippingOption>>), typeof(int), typeof(CancellationToken));
        AssertMethod(mapper, "FindByIdAsync", typeof(Task<ShippingOption?>), typeof(int), typeof(CancellationToken));
        AssertMethod(mapper, "AddAsync", typeof(Task), typeof(ShippingOption), typeof(CancellationToken));
        AssertMethod(mapper, "AddRangeAsync", typeof(Task), typeof(IEnumerable<ShippingOption>), typeof(CancellationToken));
        AssertMethod(mapper, "UpdateAsync", typeof(Task), typeof(ShippingOption), typeof(CancellationToken));
        AssertMethod(mapper, "SetCheckoutSelectedOptionAsync", typeof(Task), typeof(int), typeof(int), typeof(CancellationToken));
        AssertMethod(mapper, "SaveChangesAsync", typeof(Task), typeof(CancellationToken));

        AssertMethod(checkoutShippingContextService, "GetShippingContextAsync", typeof(Task<CheckoutShippingContext?>), typeof(int), typeof(CancellationToken));
        AssertMethod(routingService, "CreateMultiModalRouteAsync", typeof(Task<DeliveryRoute>), typeof(string), typeof(string), typeof(List<TransportMode>));
        AssertMethod(transportCarbonService, "CalculateLegCarbon", typeof(double), typeof(int), typeof(double), typeof(double), typeof(double));
        AssertMethod(transportCarbonService, "CalculateRouteCarbon", typeof(double), typeof(IReadOnlyList<double>));
        AssertMethod(transportCarbonService, "CalculateLegCarbonSurcharge", typeof(double), typeof(int), typeof(double), typeof(double), typeof(double), typeof(TransportMode));
        AssertMethod(transportCarbonService, "CalculateTotalCarbonSurcharge", typeof(double), typeof(IReadOnlyList<double>));
        AssertMethod(transportCarbonService, "CalculateRouteQuote", typeof(RouteQuoteResult), typeof(DeliveryRoute), typeof(RouteQuoteInput));

        TestAssertions.AssertEqual(typeof(IReadOnlyList<CheckoutShippingItem>), checkoutShippingContext.GetProperty(nameof(CheckoutShippingContext.Items))?.PropertyType);
        TestAssertions.AssertEqual(typeof(double), checkoutShippingContext.GetProperty(nameof(CheckoutShippingContext.TotalShipmentWeightKg))?.PropertyType);
    }

    private static void AssertMethod(Type type, string name, Type returnType, params Type[] parameterTypes)
    {
        var method = type.GetMethod(name, parameterTypes)
            ?? throw new InvalidOperationException($"{type.Name}.{name} was not found.");

        TestAssertions.AssertEqual(returnType, method.ReturnType);
    }
}

// Phase 2 covers the checkout preference strategies in isolation.
internal static class Phase2Tests
{
    public static IReadOnlyList<PhaseTest> All { get; } =
    [
        new("FAST preference strategy resolves the expected transport modes", FastShippingPreferenceStrategyResolvesExpectedModes),
        new("CHEAP preference strategy resolves the expected transport modes", CheapShippingPreferenceStrategyResolvesExpectedModes),
        new("GREEN preference strategy resolves the expected transport modes", GreenShippingPreferenceStrategyResolvesExpectedModes)
    ];

    private static void FastShippingPreferenceStrategyResolvesExpectedModes()
    {
        var strategy = new FastShippingPreferenceStrategy();

        TestAssertions.AssertSequence(new[] { TransportMode.TRAIN }, strategy.ResolveAllowedModes(isSameCountry: true));
        TestAssertions.AssertSequence(new[] { TransportMode.PLANE }, strategy.ResolveAllowedModes(isSameCountry: false));
    }

    private static void CheapShippingPreferenceStrategyResolvesExpectedModes()
    {
        var strategy = new CheapShippingPreferenceStrategy();

        TestAssertions.AssertSequence(new[] { TransportMode.TRUCK }, strategy.ResolveAllowedModes(isSameCountry: true));
        TestAssertions.AssertSequence(new[] { TransportMode.SHIP }, strategy.ResolveAllowedModes(isSameCountry: false));
    }

    private static void GreenShippingPreferenceStrategyResolvesExpectedModes()
    {
        var strategy = new GreenShippingPreferenceStrategy();

        TestAssertions.AssertSequence(new[] { TransportMode.TRAIN }, strategy.ResolveAllowedModes(isSameCountry: true));
        TestAssertions.AssertSequence(new[] { TransportMode.TRAIN, TransportMode.SHIP }, strategy.ResolveAllowedModes(isSameCountry: false));
    }
}

// Phase 3 verifies the EF-backed mapper against a real PostgreSQL test fixture.
internal static class Phase3Tests
{
    internal const string CustomerAddress = "1 Fullerton Road, Singapore 049213";
    internal const string WarehouseAddress = "8 Marina View, Singapore 018960";
    internal const string AirportAddressOne = "60 Airport Boulevard, Singapore 819643";
    internal const string AirportAddressTwo = "1-1 Furugome, Narita, Chiba 282-0004, Japan";
    internal const string ShippingPortAddressOne = "31 Marina Coastal Drive, Singapore 018988";
    internal const string ShippingPortAddressTwo = "Pier 4, North Harbor, Tondo, Manila 1012, Philippines";
    internal const string JapanCustomerAddress = "1 Chome-9 Marunouchi, Chiyoda City, Tokyo 100-0005, Japan";
    internal const string UnitedStatesCustomerAddress = "100 W 1st Street, Los Angeles, CA 90012, USA";
    internal const string UnitedStatesShippingPortAddress = "425 S Palos Verdes Street, San Pedro, CA 90731, USA";

    public static IReadOnlyList<PhaseTest> All { get; } =
    [
        new("Repository can load a checkout with selection state", RepositoryCanLoadCheckout),
        new("Repository can insert and reload shipping options for a checkout", RepositoryCanInsertAndReloadShippingOption),
        new("Repository can persist checkout.option_id selection", RepositoryCanPersistCheckoutSelection)
    ];

    private static void RepositoryCanLoadCheckout()
    {
        using var context = CreateDbContext();
        using var transaction = context.Database.BeginTransaction();

        var snapshot = CreateCheckoutFixture(context);
        var repository = new ShippingOptionMapper(context);

        var checkout = repository.FindCheckoutAsync(snapshot.CheckoutId).GetAwaiter().GetResult()
            ?? throw new InvalidOperationException("Expected repository to return a checkout.");

        var checkoutContext = checkout.GetCheckoutContext();
        TestAssertions.AssertEqual(snapshot.CustomerId, checkoutContext.CustomerId);
        TestAssertions.AssertEqual(snapshot.CartId, checkoutContext.CartId);
        TestAssertions.AssertTrue(checkout.Cart is not null, "Expected checkout cart navigation to be loaded.");

        transaction.Rollback();
    }

    private static void RepositoryCanInsertAndReloadShippingOption()
    {
        using var context = CreateDbContext();
        using var transaction = context.Database.BeginTransaction();

        var snapshot = CreateCheckoutFixture(context);
        var repository = new ShippingOptionMapper(context);
        var route = CreateRoute(context);

        var option = new ShippingOption();
        option.ConfigureGeneratedOption(snapshot.CheckoutId, route.GetRouteId(), PreferenceType.GREEN, "Phase 3 Test Option", 31.25m, 7.5, 4, TransportMode.TRAIN);

        repository.AddAsync(option).GetAwaiter().GetResult();
        repository.SaveChangesAsync().GetAwaiter().GetResult();

        var insertedSummary = option.GetSummary();
        TestAssertions.AssertTrue(insertedSummary.OptionId > 0, "Expected inserted shipping option to receive an identity value.");

        context.ChangeTracker.Clear();

        var reloaded = repository.FindByIdAsync(insertedSummary.OptionId).GetAwaiter().GetResult()
            ?? throw new InvalidOperationException("Expected inserted shipping option to be queryable by id.");
        var checkoutOptions = repository.FindByCheckoutIdAsync(snapshot.CheckoutId).GetAwaiter().GetResult();
        var reloadedSummary = reloaded.GetSummary();

        TestAssertions.AssertEqual("Phase 3 Test Option", reloadedSummary.DisplayName);
        TestAssertions.AssertEqual(PreferenceType.GREEN, reloadedSummary.PreferenceType);
        TestAssertions.AssertTrue(checkoutOptions.Any(item => item.GetSummary().OptionId == insertedSummary.OptionId), "Expected inserted option to be returned by checkout lookup.");

        transaction.Rollback();
    }

    private static void RepositoryCanPersistCheckoutSelection()
    {
        using var context = CreateDbContext();
        using var transaction = context.Database.BeginTransaction();

        var snapshot = CreateCheckoutFixture(context);
        var repository = new ShippingOptionMapper(context);
        var route = CreateRoute(context);
        var option = CreateShippingOption(snapshot.CheckoutId, route.GetRouteId(), PreferenceType.CHEAP, TransportMode.TRUCK, "Selected Option");

        repository.AddAsync(option).GetAwaiter().GetResult();
        repository.SaveChangesAsync().GetAwaiter().GetResult();
        repository.SetCheckoutSelectedOptionAsync(snapshot.CheckoutId, option.GetSummary().OptionId).GetAwaiter().GetResult();
        repository.SaveChangesAsync().GetAwaiter().GetResult();

        context.ChangeTracker.Clear();

        var checkout = context.Checkouts
            .First(entity => EF.Property<int>(entity, "Checkoutid") == snapshot.CheckoutId);

        TestAssertions.AssertEqual(option.GetSummary().OptionId, checkout.GetSelectionState().SelectedOptionId);

        transaction.Rollback();
    }

    internal static AppDbContext CreateDbContext()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' was not found.");

        var translator = new Npgsql.NameTranslation.NpgsqlNullNameTranslator();
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.MapEnum<CartStatus>("cart_status_enum", translator);
        dataSourceBuilder.MapEnum<CheckoutStatus>("checkout_status_enum", translator);
        dataSourceBuilder.MapEnum<DeliveryDuration>("delivery_duration_enum", translator);
        dataSourceBuilder.MapEnum<HubType>("hub_type", translator);
        dataSourceBuilder.MapEnum<PaymentMethod>("payment_method_enum", translator);
        dataSourceBuilder.MapEnum<OrderStatus>("order_status_enum", translator);
        dataSourceBuilder.MapEnum<ProductStatus>("product_status", translator);
        dataSourceBuilder.MapEnum<UserRole>("user_role_enum", translator);
        dataSourceBuilder.MapEnum<PreferenceType>("preference_type", translator);
        dataSourceBuilder.MapEnum<TransportMode>("transport_mode", translator);

        var dataSource = dataSourceBuilder.Build();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(dataSource, builder =>
            {
                builder.MapEnum<CartStatus>("cart_status_enum");
                builder.MapEnum<CheckoutStatus>("checkout_status_enum");
                builder.MapEnum<DeliveryDuration>("delivery_duration_enum");
                builder.MapEnum<HubType>("hub_type");
                builder.MapEnum<PaymentMethod>("payment_method_enum");
                builder.MapEnum<OrderStatus>("order_status_enum");
                builder.MapEnum<ProductStatus>("product_status");
                builder.MapEnum<UserRole>("user_role_enum");
                builder.MapEnum<PreferenceType>("preference_type");
                builder.MapEnum<TransportMode>("transport_mode");
            })
            .Options;

        return new AppDbContext(options);
    }

    internal static CheckoutFixture CreateCheckoutFixture(
        AppDbContext context,
        bool includeAdditionalOrderItem = false,
        string? customerAddress = null)
    {
        var suffix = Guid.NewGuid().ToString("N")[..12];

        var user = new User(
            name: $"Phase3 User {suffix}",
            email: $"phase3-{suffix}@example.test",
            passwordHash: "test-password-hash",
            role: UserRole.CUSTOMER);
        context.Users.Add(user);
        context.SaveChanges();

        var customer = new Customer();
        customer.SetUserId(user.GetUserId());
        customer.SetAddress(customerAddress ?? CustomerAddress);
        customer.SetCustomerType(1);
        context.Customers.Add(customer);
        context.SaveChanges();

        var cart = new Cart();
        cart.SetCustomerId(customer.GetCustomerId());
        cart.SetRentalStart(DateTime.UtcNow.Date);
        cart.SetRentalEnd(DateTime.UtcNow.Date.AddDays(3));
        context.Carts.Add(cart);
        context.SaveChanges();

        var checkout = new Checkout();
        checkout.Initialize(customer.GetCustomerId(), cart.GetCartId(), DateTime.UtcNow);
        context.Checkouts.Add(checkout);
        context.SaveChanges();

        var warehouseHubId = CreateWarehouseFixture(context, suffix);
        var (categoryId, productId) = CreateProductFixture(context, suffix);
        var productIds = new List<int> { productId };
        CreateCartItemFixture(context, cart.GetCartId(), productId);

        if (includeAdditionalOrderItem)
        {
            var (_, secondProductId) = CreateProductFixture(context, $"{suffix}-2", categoryId);
            productIds.Add(secondProductId);
            CreateCartItemFixture(context, cart.GetCartId(), secondProductId, quantity: 1);
        }

        return new CheckoutFixture(
            customer.GetCustomerId(),
            checkout.GetSelectionState().CheckoutId,
            user.GetUserId(),
            cart.GetCartId(),
            categoryId,
            productIds,
            warehouseHubId);
    }

    private static int CreateWarehouseFixture(AppDbContext context, string suffix)
    {
        var warehouse = new Warehouse();
        warehouse.SetHubType(HubType.WAREHOUSE);
        warehouse.SetAddress(WarehouseAddress);
        warehouse.SetCountryCode("SG");
        warehouse.SetLatitude(1.290270);
        warehouse.SetLongitude(103.851959);
        warehouse.SetWarehouseCode($"WH-{suffix}");
        warehouse.SetMaxProductCapacity(100);
        warehouse.SetTotalWarehouseVolume(1000d);

        context.TransportationHubs.Add(warehouse);
        context.SaveChanges();

        return warehouse.GetHubId();
    }

    internal static int CreateAirportFixture(AppDbContext context, string suffix, string address, string airportCode, string airportName)
    {
        var airport = new Airport();
        airport.SetHubType(HubType.AIRPORT);
        airport.SetAddress(address);
        airport.SetCountryCode(ResolveCountryCodeFromAddress(address));
        airport.SetLatitude(1.0);
        airport.SetLongitude(103.0);
        airport.SetAirportCode($"{airportCode}-{suffix}");
        airport.SetAirportName(airportName);

        context.TransportationHubs.Add(airport);
        context.SaveChanges();

        return airport.GetHubId();
    }

    internal static int CreateShippingPortFixture(AppDbContext context, string suffix, string address, string portCode, string portName)
    {
        var port = new ShippingPort();
        port.SetHubType(HubType.SHIPPING_PORT);
        port.SetAddress(address);
        port.SetCountryCode(ResolveCountryCodeFromAddress(address));
        port.SetLatitude(1.0);
        port.SetLongitude(103.0);
        port.SetPortCode($"{portCode}-{suffix}");
        port.SetPortName(portName);
        port.SetPortType("CONTAINER");

        context.TransportationHubs.Add(port);
        context.SaveChanges();

        return port.GetHubId();
    }

    private static string ResolveCountryCodeFromAddress(string address)
    {
        if (address.Contains("Singapore", StringComparison.OrdinalIgnoreCase))
        {
            return "SG";
        }

        if (address.Contains("Japan", StringComparison.OrdinalIgnoreCase))
        {
            return "JP";
        }

        if (address.Contains("USA", StringComparison.OrdinalIgnoreCase) ||
            address.Contains("United States", StringComparison.OrdinalIgnoreCase))
        {
            return "US";
        }

        if (address.Contains("Philippines", StringComparison.OrdinalIgnoreCase))
        {
            return "PH";
        }

        throw new InvalidOperationException($"Unsupported test country mapping for address '{address}'.");
    }

    private static (int CategoryId, int ProductId) CreateProductFixture(AppDbContext context, string suffix, int? categoryId = null)
    {
        var now = DateTime.UtcNow;
        var resolvedCategoryId = categoryId;

        if (!resolvedCategoryId.HasValue)
        {
            var category = new Category();
            SetPrivateField(category, "_name", $"Phase 3 Category {suffix}");
            SetPrivateField(category, "_description", "Feature 1 test category");
            SetPrivateField(category, "_createddate", now);
            SetPrivateField(category, "_updateddate", now);
            context.Categories.Add(category);
            context.SaveChanges();

            resolvedCategoryId = GetPrivateField<int>(category, "_categoryid");
        }

        var product = new Product();
        SetPrivateField(product, "_categoryid", resolvedCategoryId.Value);
        SetPrivateField(product, "_sku", $"PHASE3-SKU-{suffix}");
        SetPrivateField(product, "_threshold", 0.1000m);
        SetPrivateField(product, "_createdat", now);
        SetPrivateField(product, "_updatedat", now);
        product.UpdateStatus(ProductStatus.AVAILABLE);
        context.Products.Add(product);
        context.SaveChanges();

        return (resolvedCategoryId.Value, GetPrivateField<int>(product, "_productid"));
    }

    internal static void CreateCartItemFixture(AppDbContext context, int cartId, int productId, int quantity = 2)
    {
        var cartItem = new Cartitem();
        SetPrivateField(cartItem, "_cartid", cartId);
        SetPrivateField(cartItem, "_productid", productId);
        SetPrivateField(cartItem, "_quantity", quantity);
        SetPrivateField(cartItem, "_isselected", true);

        context.Cartitems.Add(cartItem);
        context.SaveChanges();
    }

    private static void SetPrivateField<TTarget, TValue>(TTarget target, string fieldName, TValue value)
    {
        var field = typeof(TTarget).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException($"Field '{fieldName}' was not found on {typeof(TTarget).Name}.");
        field.SetValue(target, value);
    }

    private static TValue GetPrivateField<TValue>(object target, string fieldName)
    {
        var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException($"Field '{fieldName}' was not found on {target.GetType().Name}.");
        return (TValue)(field.GetValue(target)
            ?? throw new InvalidOperationException($"Field '{fieldName}' on {target.GetType().Name} was null."));
    }

    private static DeliveryRoute CreateRoute(AppDbContext context)
    {
        var route = new DeliveryRoute();
        route.SetOriginAddress("Phase 3 Warehouse");
        route.SetDestinationAddress("Phase 3 Destination");
        route.SetTotalDistanceKm(12.5);
        route.SetIsValid(true);

        context.DeliveryRoutes.Add(route);
        context.SaveChanges();

        return route;
    }

    private static ShippingOption CreateShippingOption(
        int checkoutId,
        int routeId,
        PreferenceType preferenceType,
        TransportMode transportMode,
        string displayName)
    {
        var option = new ShippingOption();
        option.ConfigureGeneratedOption(checkoutId, routeId, preferenceType, displayName, 19.99m, 5.5, 3, transportMode);
        return option;
    }

    internal sealed record CheckoutFixture(
        int CustomerId,
        int CheckoutId,
        int UserId,
        int CartId,
        int CategoryId,
        IReadOnlyList<int> ProductIds,
        int WarehouseHubId)
    {
        public int ProductId => ProductIds[0];
    }

}

// Phase 4 exercises ShippingOptionManager with in-memory test doubles to validate orchestration rules.
internal static class Phase4Tests
{
    public static IReadOnlyList<PhaseTest> All { get; } =
    [
        new("TransportCarbonManager calculates leg surcharge by transport type", TransportCarbonManagerCalculatesLegSurchargeByTransportType),
        new("TransportCarbonManager sums leg surcharges into route total", TransportCarbonManagerSumsLegSurcharges),
        new("RouteDistanceCalculator returns Google distance when route lookup succeeds", RouteDistanceCalculatorReturnsGoogleDistance),
        new("RouteDistanceCalculator fails when Google returns no route distance", RouteDistanceCalculatorThrowsWhenGoogleReturnsNoRoute),
        new("RouteDistanceCalculator fails when Google returns a non-success status", RouteDistanceCalculatorThrowsWhenGoogleReturnsNonSuccessStatus),
        new("RouteDistanceCalculator uses geodesic distance for PLANE main legs without Google", RouteDistanceCalculatorUsesGeodesicDistanceForPlaneMainLegs),
        new("RouteDistanceCalculator uses geodesic distance for SHIP main legs without Google", RouteDistanceCalculatorUsesGeodesicDistanceForShipMainLegs),
        new("RouteDistanceCalculator fails when Google API key is missing", RouteDistanceCalculatorThrowsWhenGoogleApiKeyIsMissing),
        new("RouteDistanceCalculator fails when route endpoints are blank", RouteDistanceCalculatorThrowsWhenRouteEndpointsAreBlank),
        new("RouteDistanceCalculator fails when PLANE main-leg coordinates are missing", RouteDistanceCalculatorThrowsWhenPlaneCoordinatesAreMissing),
        new("RouteDistanceCalculator fails when SHIP main-leg coordinates are invalid", RouteDistanceCalculatorThrowsWhenShipCoordinatesAreInvalid),
        new("PreferenceManager returns same-country preference cards", PreferenceManagerReturnsPreferenceCardsWithoutRouting),
        new("PreferenceManager returns cross-country preference cards with PLANE and SHIP labels", PreferenceManagerReturnsCrossCountryPreferenceCards),
        new("PreferenceManager rejects duplicate shipping preference strategies", PreferenceManagerRejectsDuplicateStrategies),
        new("PreferenceManager rejects missing shipping preference strategies", PreferenceManagerRejectsMissingStrategies),
        new("PreferenceManager resolves one selected preference with metadata and modes", PreferenceManagerResolvesSelectedPreference),
        new("ShippingOptionManager delegates card construction to the preference service", ShippingOptionManagerDelegatesCardConstruction),
        new("ShippingOptionManager confirms one preference with one route and one quote call", ShippingOptionManagerConfirmsPreferenceAndPersistsSingleOption),
        new("ShippingOptionManager uses the resolved preference modes and metadata", ShippingOptionManagerUsesResolvedPreferenceModesAndMetadata),
        new("ShippingOptionManager reuses an existing persisted option on reselection", ShippingOptionManagerReusesPersistedOptionOnReselection)
    ];

    private static void TransportCarbonManagerCalculatesLegSurchargeByTransportType()
    {
        var manager = new ProRental.Domain.Module3.P2_1.Controls.TransportCarbonManager(new StubPricingRuleGateway(), new StubHubCarbonService());

        var truckSurcharge = manager.CalculateLegCarbonSurcharge(2, 5.0, 10.0, 3.0, TransportMode.TRUCK);
        var shipSurcharge = manager.CalculateLegCarbonSurcharge(2, 5.0, 10.0, 3.0, TransportMode.SHIP);
        var planeSurcharge = manager.CalculateLegCarbonSurcharge(2, 5.0, 10.0, 3.0, TransportMode.PLANE);
        var trainSurcharge = manager.CalculateLegCarbonSurcharge(2, 5.0, 10.0, 3.0, TransportMode.TRAIN);

        TestAssertions.AssertEqual(5.15d, truckSurcharge);
        TestAssertions.AssertEqual(3.09d, shipSurcharge);
        TestAssertions.AssertEqual(12.36d, planeSurcharge);
        TestAssertions.AssertEqual(4.12d, trainSurcharge);
    }

    private static void TransportCarbonManagerSumsLegSurcharges()
    {
        var manager = new ProRental.Domain.Module3.P2_1.Controls.TransportCarbonManager(new StubPricingRuleGateway(), new StubHubCarbonService());

        var truckSurcharge = manager.CalculateLegCarbonSurcharge(2, 5.0, 10.0, 3.0, TransportMode.TRUCK);
        var shipSurcharge = manager.CalculateLegCarbonSurcharge(2, 5.0, 10.0, 3.0, TransportMode.SHIP);
        var planeSurcharge = manager.CalculateLegCarbonSurcharge(2, 5.0, 10.0, 3.0, TransportMode.PLANE);

        var total = manager.CalculateTotalCarbonSurcharge([truckSurcharge, shipSurcharge, planeSurcharge]);

        TestAssertions.AssertEqual(20.6d, total);
    }

    private static void RouteDistanceCalculatorReturnsGoogleDistance()
    {
        var calculator = new RouteDistanceCalculator(CreateGoogleMapsApi(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new
            {
                routes = new[]
                {
                    new
                    {
                        distanceMeters = 12345d
                    }
                }
            })
        }));

        var distanceKm = calculator.CalculateLegDistanceKmAsync(
            TransportMode.TRUCK,
            CreatePoint("8 Marina View, Singapore 018960"),
            CreatePoint("1 Fullerton Road, Singapore 049213")).GetAwaiter().GetResult();

        TestAssertions.AssertEqual(12.35d, distanceKm);
    }

    private static void RouteDistanceCalculatorThrowsWhenGoogleReturnsNoRoute()
    {
        var calculator = new RouteDistanceCalculator(CreateGoogleMapsApi(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new
            {
                routes = Array.Empty<object>()
            })
        }));

        var exception = TestAssertions.AssertThrows<RouteResolutionException>(
            () => calculator.CalculateLegDistanceKmAsync(
                TransportMode.TRUCK,
                CreatePoint("8 Marina View, Singapore 018960"),
                CreatePoint("1 Fullerton Road, Singapore 049213")).GetAwaiter().GetResult(),
            "Expected route distance lookup to fail when Google returns no route.");

        TestAssertions.AssertTrue(
            exception.Message.Contains("did not return a route distance", StringComparison.Ordinal),
            "Expected a missing-route error message.");
    }

    private static void RouteDistanceCalculatorThrowsWhenGoogleReturnsNonSuccessStatus()
    {
        var calculator = new RouteDistanceCalculator(CreateGoogleMapsApi(_ => new HttpResponseMessage(HttpStatusCode.BadGateway)));

        var exception = TestAssertions.AssertThrows<RouteResolutionException>(
            () => calculator.CalculateLegDistanceKmAsync(
                TransportMode.TRUCK,
                CreatePoint("8 Marina View, Singapore 018960"),
                CreatePoint("1 Fullerton Road, Singapore 049213")).GetAwaiter().GetResult(),
            "Expected route distance lookup to fail when Google returns a non-success status.");

        TestAssertions.AssertTrue(
            exception.Message.Contains("status", StringComparison.Ordinal),
            "Expected an HTTP-status error message.");
    }

    private static void RouteDistanceCalculatorUsesGeodesicDistanceForPlaneMainLegs()
    {
        var googleMapsApi = new Phase4RecordingGoogleMapsApi();
        var calculator = new RouteDistanceCalculator(googleMapsApi);

        var distanceKm = calculator.CalculateLegDistanceKmAsync(
            TransportMode.PLANE,
            CreatePoint("Origin Airport", 0d, 0d),
            CreatePoint("Destination Airport", 0d, 1d)).GetAwaiter().GetResult();

        TestAssertions.AssertEqual(111.2d, distanceKm);
        TestAssertions.AssertEqual(0, googleMapsApi.Requests.Count);
    }

    private static void RouteDistanceCalculatorUsesGeodesicDistanceForShipMainLegs()
    {
        var googleMapsApi = new Phase4RecordingGoogleMapsApi();
        var calculator = new RouteDistanceCalculator(googleMapsApi);

        var distanceKm = calculator.CalculateLegDistanceKmAsync(
            TransportMode.SHIP,
            CreatePoint("Origin Port", 0d, 0d),
            CreatePoint("Destination Port", 1d, 0d)).GetAwaiter().GetResult();

        TestAssertions.AssertEqual(111.2d, distanceKm);
        TestAssertions.AssertEqual(0, googleMapsApi.Requests.Count);
    }

    private static void RouteDistanceCalculatorThrowsWhenGoogleApiKeyIsMissing()
    {
        var calculator = new RouteDistanceCalculator(CreateGoogleMapsApi(
            _ => throw new InvalidOperationException("Google route lookup should not run without an API key."),
            apiKey: string.Empty));

        var exception = TestAssertions.AssertThrows<RouteResolutionException>(
            () => calculator.CalculateLegDistanceKmAsync(
                TransportMode.TRUCK,
                CreatePoint("8 Marina View, Singapore 018960"),
                CreatePoint("1 Fullerton Road, Singapore 049213")).GetAwaiter().GetResult(),
            "Expected route distance lookup to fail when the Google API key is missing.");

        TestAssertions.AssertTrue(
            exception.Message.Contains("configured API key and non-empty route endpoints", StringComparison.Ordinal),
            "Expected a missing-key configuration error message.");
    }

    private static void RouteDistanceCalculatorThrowsWhenRouteEndpointsAreBlank()
    {
        var calculator = new RouteDistanceCalculator(CreateGoogleMapsApi(
            _ => throw new InvalidOperationException("Google route lookup should not run with blank route endpoints.")));

        var exception = TestAssertions.AssertThrows<RouteResolutionException>(
            () => calculator.CalculateLegDistanceKmAsync(
                TransportMode.TRUCK,
                CreatePoint(string.Empty),
                CreatePoint("1 Fullerton Road, Singapore 049213")).GetAwaiter().GetResult(),
            "Expected route distance lookup to fail when route endpoints are blank.");

        TestAssertions.AssertTrue(
            exception.Message.Contains("configured API key and non-empty route endpoints", StringComparison.Ordinal),
            "Expected a blank-endpoint configuration error message.");
    }

    private static void RouteDistanceCalculatorThrowsWhenPlaneCoordinatesAreMissing()
    {
        var googleMapsApi = new Phase4RecordingGoogleMapsApi();
        var calculator = new RouteDistanceCalculator(googleMapsApi);

        var exception = TestAssertions.AssertThrows<RouteResolutionException>(
            () => calculator.CalculateLegDistanceKmAsync(
                TransportMode.PLANE,
                CreatePoint("Origin Airport"),
                CreatePoint("Destination Airport", 0d, 1d)).GetAwaiter().GetResult(),
            "Expected PLANE main-leg distance lookup to fail without coordinates.");

        TestAssertions.AssertTrue(
            exception.Message.Contains("Valid hub coordinates", StringComparison.Ordinal),
            "Expected a missing-coordinate error message.");
        TestAssertions.AssertEqual(0, googleMapsApi.Requests.Count);
    }

    private static void RouteDistanceCalculatorThrowsWhenShipCoordinatesAreInvalid()
    {
        var googleMapsApi = new Phase4RecordingGoogleMapsApi();
        var calculator = new RouteDistanceCalculator(googleMapsApi);

        var exception = TestAssertions.AssertThrows<RouteResolutionException>(
            () => calculator.CalculateLegDistanceKmAsync(
                TransportMode.SHIP,
                CreatePoint("Origin Port", 95d, 0d),
                CreatePoint("Destination Port", 1d, 0d)).GetAwaiter().GetResult(),
            "Expected SHIP main-leg distance lookup to fail with invalid coordinates.");

        TestAssertions.AssertTrue(
            exception.Message.Contains("Valid hub coordinates", StringComparison.Ordinal),
            "Expected an invalid-coordinate error message.");
        TestAssertions.AssertEqual(0, googleMapsApi.Requests.Count);
    }

    private static GoogleMapsAPI CreateGoogleMapsApi(Func<HttpRequestMessage, HttpResponseMessage> responder, string apiKey = "test-key")
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [GoogleMapsConfigurationDiagnostics.ApiKeyConfigPath] = apiKey
            })
            .Build();

        var httpClient = new HttpClient(new Phase4StubHttpMessageHandler(responder))
        {
            BaseAddress = new Uri("https://routes.googleapis.com/")
        };

        return new GoogleMapsAPI(httpClient, configuration);
    }

    private static RouteDistancePoint CreatePoint(string address, double? latitude = null, double? longitude = null) =>
        new(address, latitude, longitude);

    private static void PreferenceManagerReturnsPreferenceCardsWithoutRouting()
    {
        var manager = CreatePreferenceService();
        var context = CreateCheckoutShippingContext();

        var result = manager.BuildPreferenceCards(context, isSameCountry: true);

        TestAssertions.AssertEqual(3, result.Count);
        TestAssertions.AssertSequence(
            new[] { PreferenceType.FAST, PreferenceType.CHEAP, PreferenceType.GREEN },
            result.Select(option => option.PreferenceType));
        TestAssertions.AssertEqual("Fastest", result.Single(option => option.PreferenceType == PreferenceType.FAST).DisplayName);
        TestAssertions.AssertEqual("Cheapest", result.Single(option => option.PreferenceType == PreferenceType.CHEAP).DisplayName);
        TestAssertions.AssertEqual("Greenest", result.Single(option => option.PreferenceType == PreferenceType.GREEN).DisplayName);
        TestAssertions.AssertTrue(
            result.Single(option => option.PreferenceType == PreferenceType.FAST).Description.Contains("time-sensitive", StringComparison.Ordinal),
            "Expected FAST card description to come from preference metadata.");
        TestAssertions.AssertEqual("TRAIN", result.Single(option => option.PreferenceType == PreferenceType.FAST).AllowedModesLabel);
        TestAssertions.AssertEqual("TRUCK", result.Single(option => option.PreferenceType == PreferenceType.CHEAP).AllowedModesLabel);
        TestAssertions.AssertEqual("TRAIN", result.Single(option => option.PreferenceType == PreferenceType.GREEN).AllowedModesLabel);
    }

    private static void PreferenceManagerReturnsCrossCountryPreferenceCards()
    {
        var manager = CreatePreferenceService();
        var context = CreateCheckoutShippingContext(Phase3Tests.JapanCustomerAddress);

        var result = manager.BuildPreferenceCards(context, isSameCountry: false);

        TestAssertions.AssertEqual("PLANE", result.Single(option => option.PreferenceType == PreferenceType.FAST).AllowedModesLabel);
        TestAssertions.AssertEqual("SHIP", result.Single(option => option.PreferenceType == PreferenceType.CHEAP).AllowedModesLabel);
        TestAssertions.AssertEqual("TRAIN + SHIP", result.Single(option => option.PreferenceType == PreferenceType.GREEN).AllowedModesLabel);
    }

    private static void PreferenceManagerRejectsDuplicateStrategies()
    {
        var exception = TestAssertions.AssertThrows<InvalidOperationException>(
            () => new PreferenceManager(
                [new FastShippingPreferenceStrategy(), new FastShippingPreferenceStrategy(), new GreenShippingPreferenceStrategy()]),
            "Expected duplicate preference strategies to be rejected.");

        TestAssertions.AssertTrue(
            exception.Message.Contains("Duplicate shipping preference strategy registration", StringComparison.Ordinal),
            "Expected duplicate strategy registration message.");
    }

    private static void PreferenceManagerRejectsMissingStrategies()
    {
        var exception = TestAssertions.AssertThrows<InvalidOperationException>(
            () => new PreferenceManager(
                [new FastShippingPreferenceStrategy(), new CheapShippingPreferenceStrategy()]),
            "Expected missing preference strategies to be rejected.");

        TestAssertions.AssertTrue(
            exception.Message.Contains("Missing shipping preference strategies", StringComparison.Ordinal),
            "Expected missing strategy registration message.");
    }

    private static void PreferenceManagerResolvesSelectedPreference()
    {
        var manager = CreatePreferenceService();

        var result = manager.ResolvePreference(PreferenceType.GREEN, isSameCountry: false);

        TestAssertions.AssertEqual(PreferenceType.GREEN, result.PreferenceType);
        TestAssertions.AssertEqual(3, result.DisplayPreference);
        TestAssertions.AssertEqual("Greenest", result.DisplayName);
        TestAssertions.AssertTrue(
            result.Description.Contains("balanced", StringComparison.OrdinalIgnoreCase),
            "Expected the GREEN preference description to be carried in the resolved preference.");
        TestAssertions.AssertEqual(4, result.DeliveryDays);
        TestAssertions.AssertSequence(new[] { TransportMode.TRAIN, TransportMode.SHIP }, result.AllowedModes);
    }

    private static void ShippingOptionManagerDelegatesCardConstruction()
    {
        var repository = new InMemoryShippingOptionMapper();
        var checkoutShippingContextService = new StubCheckoutShippingContextService(CreateCheckoutShippingContext());
        var preferenceService = new StubShippingPreferenceService(
            cards:
            [
                new ShippingPreferenceCard(30, PreferenceType.FAST, "Priority", "Delegated card", "PLANE"),
                new ShippingPreferenceCard(30, PreferenceType.CHEAP, "Saver", "Delegated card", "SHIP"),
                new ShippingPreferenceCard(30, PreferenceType.GREEN, "Eco", "Delegated card", "TRAIN")
            ],
            resolvedPreference: CreateResolvedPreference(PreferenceType.GREEN, "Eco", 4, [TransportMode.TRAIN]));
        var routingService = new StubRoutingService();
        var transportCarbonService = new StubTransportCarbonService();
        var manager = new ShippingOptionManager(
            repository,
            checkoutShippingContextService,
            preferenceService,
            routingService,
            CreatePhase4HubMapper(),
            transportCarbonService);

        var result = manager.GetPreferenceChoicesForCheckoutAsync(checkoutShippingContextService.Context.CheckoutId).GetAwaiter().GetResult();

        TestAssertions.AssertEqual(30, preferenceService.LastBuildContext?.CheckoutId);
        TestAssertions.AssertEqual(true, preferenceService.LastBuildIsSameCountry);
        TestAssertions.AssertSequence(new[] { "Priority", "Saver", "Eco" }, result.Select(option => option.DisplayName));
        TestAssertions.AssertEqual(0, repository.StoredOptions.Count);
        TestAssertions.AssertEqual(0, routingService.Requests.Count);
        TestAssertions.AssertEqual(0, transportCarbonService.RouteQuoteRequests.Count);
    }

    private static void ShippingOptionManagerConfirmsPreferenceAndPersistsSingleOption()
    {
        var repository = new InMemoryShippingOptionMapper();
        repository.Checkout = CreateCheckout(30, 20);
        var checkoutShippingContextService = new StubCheckoutShippingContextService(CreateCheckoutShippingContext());
        var preferenceService = CreatePreferenceService();
        var routingService = new StubRoutingService();
        var transportCarbonService = new StubTransportCarbonService();
        var manager = new ShippingOptionManager(
            repository,
            checkoutShippingContextService,
            preferenceService,
            routingService,
            CreatePhase4HubMapper(),
            transportCarbonService);

        var result = manager.ConfirmPreferenceSelectionAsync(
            new SelectShippingPreferenceRequest(30, PreferenceType.GREEN)).GetAwaiter().GetResult();

        TestAssertions.AssertEqual(30, repository.LastSelectedCheckoutId);
        TestAssertions.AssertEqual(1, repository.LastSelectedOptionId);
        TestAssertions.AssertEqual(1, repository.StoredOptions.Count);
        TestAssertions.AssertEqual(1, routingService.Requests.Count);
        TestAssertions.AssertEqual(1, transportCarbonService.RouteQuoteRequests.Count);
        TestAssertions.AssertSequence(new[] { TransportMode.TRAIN }, routingService.Requests.Single().Modes);
        TestAssertions.AssertEqual(PreferenceType.GREEN, result.PreferenceType);
        TestAssertions.AssertEqual("TRAIN", result.TransportModeLabel);

        var quoteRequest = transportCarbonService.RouteQuoteRequests.Single();
        TestAssertions.AssertEqual(202, quoteRequest.QuoteInput.HubId);
        TestAssertions.AssertEqual(2, quoteRequest.QuoteInput.Items.Count);
        TestAssertions.AssertEqual(16.5d, quoteRequest.QuoteInput.Items.Sum(item => item.Quantity * item.UnitWeightKg));
    }

    private static void ShippingOptionManagerUsesResolvedPreferenceModesAndMetadata()
    {
        var repository = new InMemoryShippingOptionMapper();
        repository.Checkout = CreateCheckout(30, 20);
        var checkoutShippingContextService = new StubCheckoutShippingContextService(CreateCheckoutShippingContext());
        var preferenceService = new StubShippingPreferenceService(
            cards: Array.Empty<ShippingPreferenceCard>(),
            resolvedPreference: CreateResolvedPreference(PreferenceType.FAST, "Priority Air", 9, [TransportMode.PLANE]));
        var routingService = new StubRoutingService();
        var transportCarbonService = new StubTransportCarbonService();
        var manager = new ShippingOptionManager(
            repository,
            checkoutShippingContextService,
            preferenceService,
            routingService,
            CreatePhase4HubMapper(),
            transportCarbonService);

        var result = manager.ConfirmPreferenceSelectionAsync(
            new SelectShippingPreferenceRequest(30, PreferenceType.FAST)).GetAwaiter().GetResult();

        TestAssertions.AssertEqual(PreferenceType.FAST, preferenceService.LastResolvedPreferenceType);
        TestAssertions.AssertEqual(true, preferenceService.LastResolveIsSameCountry);
        TestAssertions.AssertSequence(new[] { TransportMode.PLANE }, routingService.Requests.Single().Modes);
        TestAssertions.AssertEqual("PLANE", result.TransportModeLabel);
        var summary = repository.StoredOptions.Single().GetSummary();
        TestAssertions.AssertEqual("Priority Air", summary.DisplayName);
        TestAssertions.AssertEqual(TransportMode.PLANE, summary.TransportMode);
        TestAssertions.AssertEqual(9, summary.DeliveryDays);
    }

    private static void ShippingOptionManagerReusesPersistedOptionOnReselection()
    {
        var repository = new InMemoryShippingOptionMapper();
        repository.Checkout = CreateCheckout(30, 20);
        repository.Seed(CreatePersistedOption(5, 30, PreferenceType.CHEAP, "Cheapest", 12m, 3.2, 5, TransportMode.SHIP));

        var routingService = new StubRoutingService();
        var transportCarbonService = new StubTransportCarbonService();
        var preferenceService = CreatePreferenceService();
        var manager = new ShippingOptionManager(
            repository,
            new StubCheckoutShippingContextService(CreateCheckoutShippingContext(Phase3Tests.JapanCustomerAddress)),
            preferenceService,
            routingService,
            CreatePhase4HubMapper(),
            transportCarbonService);

        var selection = manager.ConfirmPreferenceSelectionAsync(
            new SelectShippingPreferenceRequest(30, PreferenceType.FAST)).GetAwaiter().GetResult();

        TestAssertions.AssertEqual(30, repository.LastSelectedCheckoutId);
        TestAssertions.AssertEqual(5, repository.LastSelectedOptionId);
        TestAssertions.AssertEqual(1, repository.StoredOptions.Count);
        TestAssertions.AssertEqual(1, routingService.Requests.Count);
        TestAssertions.AssertEqual(1, transportCarbonService.RouteQuoteRequests.Count);
        TestAssertions.AssertEqual(PreferenceType.FAST, selection.PreferenceType);
        TestAssertions.AssertEqual("PLANE", selection.TransportModeLabel);
    }

    private static CheckoutShippingContext CreateCheckoutShippingContext(string destinationAddress = "Singapore")
    {
        CheckoutShippingItem[] items =
        [
            new CheckoutShippingItem(101, 2, 5.5d),
            new CheckoutShippingItem(102, 1, 5.5d)
        ];

        return new CheckoutShippingContext(30, 20, destinationAddress, 202, items, 16.5d);
    }

    internal static IReadOnlyList<IShippingPreferenceStrategy> CreatePreferenceStrategies() =>
    [
        new FastShippingPreferenceStrategy(),
        new CheapShippingPreferenceStrategy(),
        new GreenShippingPreferenceStrategy()
    ];

    internal static IShippingPreferenceService CreatePreferenceService() =>
        new PreferenceManager(CreatePreferenceStrategies());

    private static ResolvedShippingPreference CreateResolvedPreference(
        PreferenceType preferenceType,
        string displayName,
        int deliveryDays,
        IReadOnlyList<TransportMode> allowedModes) =>
        new(
            preferenceType,
            preferenceType switch
            {
                PreferenceType.FAST => 1,
                PreferenceType.CHEAP => 2,
                _ => 3
            },
            displayName,
            $"{displayName} description",
            deliveryDays,
            allowedModes);

    private static ITransportationHubMapper CreatePhase4HubMapper()
    {
        return new Phase4TransportationHubMapper([CreatePhase4WarehouseHub(202, Phase3Tests.WarehouseAddress)]);
    }

    private static Warehouse CreatePhase4WarehouseHub(int hubId, string address)
    {
        var warehouse = new Warehouse();
        warehouse.SetHubId(hubId);
        warehouse.SetHubType(HubType.WAREHOUSE);
        warehouse.SetAddress(address);
        warehouse.SetCountryCode("SG");
        warehouse.SetLatitude(1.290270);
        warehouse.SetLongitude(103.851959);
        warehouse.SetWarehouseCode($"PHASE4-WH-{hubId}");
        warehouse.SetMaxProductCapacity(100);
        warehouse.SetTotalWarehouseVolume(1000d);
        return warehouse;
    }

    private static ShippingOption CreatePersistedOption(
        int optionId,
        int checkoutId,
        PreferenceType preferenceType,
        string displayName,
        decimal cost,
        double carbonFootprintKg,
        int deliveryDays,
        TransportMode transportMode)
    {
        var option = new ShippingOption();
        option.ConfigureGeneratedOption(checkoutId, null, preferenceType, displayName, cost, carbonFootprintKg, deliveryDays, transportMode);
        SetPrivateField(option, "_optionId", optionId);
        return option;
    }

    private static Checkout CreateCheckout(int checkoutId, int customerId)
    {
        var checkout = new Checkout();
        checkout.Initialize(customerId, 40, DateTime.UtcNow);
        SetPrivateField(checkout, "_checkoutid", checkoutId);
        return checkout;
    }

    private static void SetPrivateField<TTarget, TValue>(TTarget target, string fieldName, TValue value)
    {
        var field = typeof(TTarget).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException($"Field '{fieldName}' was not found on {typeof(TTarget).Name}.");
        field.SetValue(target, value);
    }

    // In-memory repository double used to verify manager behavior without EF or PostgreSQL.
    private sealed class InMemoryShippingOptionMapper : IShippingOptionMapper
    {
        public List<ShippingOption> StoredOptions { get; } = [];
        public Checkout? Checkout { get; set; }
        public int? LastSelectedCheckoutId { get; private set; }
        public int? LastSelectedOptionId { get; private set; }
        private int _nextOptionId = 1;

        public Task<Checkout?> FindCheckoutAsync(int checkoutId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Checkout is not null && Checkout.GetSelectionState().CheckoutId == checkoutId ? Checkout : null);
        }

        public Task<IReadOnlyList<ShippingOption>> FindByCheckoutIdAsync(int checkoutId, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<ShippingOption> options = StoredOptions.Where(option => option.BelongsToCheckout(checkoutId)).ToArray();
            return Task.FromResult(options);
        }

        public Task<ShippingOption?> FindByIdAsync(int optionId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(StoredOptions.FirstOrDefault(option => option.GetSummary().OptionId == optionId));
        }

        public Task<int?> FindSelectedRouteIdByOrderIdAsync(int orderId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<int?>(null);
        }

        public Task AddAsync(ShippingOption option, CancellationToken cancellationToken = default)
        {
            if (option.GetSummary().OptionId == 0)
            {
                SetPrivateField(option, "_optionId", _nextOptionId++);
            }

            StoredOptions.Add(option);
            return Task.CompletedTask;
        }

        public Task AddRangeAsync(IEnumerable<ShippingOption> options, CancellationToken cancellationToken = default)
        {
            foreach (var option in options)
            {
                AddAsync(option, cancellationToken).GetAwaiter().GetResult();
            }

            return Task.CompletedTask;
        }

        public Task UpdateAsync(ShippingOption option, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task SetCheckoutSelectedOptionAsync(int checkoutId, int optionId, CancellationToken cancellationToken = default)
        {
            LastSelectedCheckoutId = checkoutId;
            LastSelectedOptionId = optionId;
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public void Seed(ShippingOption option)
        {
            StoredOptions.Add(option);
            _nextOptionId = Math.Max(_nextOptionId, option.GetSummary().OptionId + 1);
        }
    }

    // Stub Module 1 adapter that returns a fixed order context for manager tests.
    private sealed class StubCheckoutShippingContextService : ICheckoutShippingContextService
    {
        public StubCheckoutShippingContextService(CheckoutShippingContext context)
        {
            Context = context;
        }

        public CheckoutShippingContext Context { get; }
        public int CallCount { get; private set; }

        public Task<CheckoutShippingContext?> GetShippingContextAsync(int checkoutId, CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.FromResult<CheckoutShippingContext?>(Context.CheckoutId == checkoutId ? Context : null);
        }
    }

    // Stub preference context used to isolate ShippingOptionManager orchestration from preference resolution.
    private sealed class StubShippingPreferenceService : IShippingPreferenceService
    {
        private readonly IReadOnlyList<ShippingPreferenceCard> _cards;
        private readonly ResolvedShippingPreference _resolvedPreference;

        public StubShippingPreferenceService(
            IReadOnlyList<ShippingPreferenceCard> cards,
            ResolvedShippingPreference resolvedPreference)
        {
            _cards = cards;
            _resolvedPreference = resolvedPreference;
        }

        public CheckoutShippingContext? LastBuildContext { get; private set; }
        public bool? LastBuildIsSameCountry { get; private set; }
        public PreferenceType? LastResolvedPreferenceType { get; private set; }
        public bool? LastResolveIsSameCountry { get; private set; }

        public IReadOnlyList<ShippingPreferenceCard> BuildPreferenceCards(CheckoutShippingContext context, bool isSameCountry)
        {
            LastBuildContext = context;
            LastBuildIsSameCountry = isSameCountry;
            return _cards;
        }

        public ResolvedShippingPreference ResolvePreference(PreferenceType preferenceType, bool isSameCountry)
        {
            LastResolvedPreferenceType = preferenceType;
            LastResolveIsSameCountry = isSameCountry;
            return _resolvedPreference;
        }
    }

    // Stub Feature 3 adapter that returns one deterministic route for a selected preference.
    private sealed class StubRoutingService : IRoutingService
    {
        public List<(string Origin, string Destination, IReadOnlyList<TransportMode> Modes)> Requests { get; } = [];

        public Task<DeliveryRoute> CreateMultiModalRouteAsync(string origin, string destination, List<TransportMode> modes)
        {
            Requests.Add((origin, destination, modes.ToArray()));
            var route = new DeliveryRoute();
            route.SetOriginAddress(origin);
            route.SetDestinationAddress(destination);
            route.SetIsValid(true);
            route.SetTotalDistanceKm(24d);

            var primaryMode = modes.FirstOrDefault();
            if (primaryMode is TransportMode.TRAIN or TransportMode.TRUCK)
            {
                var directLeg = new RouteLeg();
                directLeg.ConfigureLeg(1, origin, destination, 24d, primaryMode, false, true, false);
                route.RouteLegs.Add(directLeg);
            }
            else
            {
                var firstLeg = new RouteLeg();
                firstLeg.ConfigureLeg(1, origin, "Departure Hub", 4d, TransportMode.TRUCK, true, false, false);
                route.RouteLegs.Add(firstLeg);

                var mainLeg = new RouteLeg();
                mainLeg.ConfigureLeg(2, "Departure Hub", "Arrival Hub", 16d, primaryMode, false, true, false);
                route.RouteLegs.Add(mainLeg);

                var lastLeg = new RouteLeg();
                lastLeg.ConfigureLeg(3, "Arrival Hub", destination, 4d, TransportMode.TRUCK, false, false, true);
                route.RouteLegs.Add(lastLeg);
            }

            return Task.FromResult(route);
        }
    }

    private sealed class Phase4TransportationHubMapper : ITransportationHubMapper
    {
        private readonly List<TransportationHub> _hubs;

        public Phase4TransportationHubMapper(IEnumerable<TransportationHub> hubs)
        {
            _hubs = hubs.ToList();
        }

        public TransportationHub? FindById(int hubId) => _hubs.FirstOrDefault(hub => hub.GetHubId() == hubId);
        public List<TransportationHub> FindByType(HubType hubType) => _hubs.Where(hub => hub.GetHubType() == hubType).ToList();
        public List<TransportationHub> FindAll() => _hubs.ToList();
        public void Insert(TransportationHub hub) => _hubs.Add(hub);
        public void Update(TransportationHub hub) { }
        public void Delete(int hubId) => _hubs.RemoveAll(hub => hub.GetHubId() == hubId);
    }

    // Stub Feature 2 adapter that exposes both the low-level carbon primitives and the route-level quote call.
    private sealed class StubTransportCarbonService : ITransportCarbonService
    {
        public List<(int Quantity, double WeightKg, double DistanceKm, double StorageCo2)> LegRequests { get; } = [];
        public List<IReadOnlyList<double>> RouteRequests { get; } = [];
        public List<(int Quantity, double WeightKg, double DistanceKm, double StorageCo2, TransportMode TransportMode)> LegSurchargeRequests { get; } = [];
        public List<IReadOnlyList<double>> TotalSurchargeRequests { get; } = [];
        public List<(DeliveryRoute Route, RouteQuoteInput QuoteInput)> RouteQuoteRequests { get; } = [];

        public double CalculateLegCarbon(int quantity, double weightKg, double distanceKm, double storageCo2)
        {
            LegRequests.Add((quantity, weightKg, distanceKm, storageCo2));
            return (quantity * weightKg * distanceKm) + storageCo2;
        }

        public double CalculateRouteCarbon(IReadOnlyList<double> legCarbonValues)
        {
            RouteRequests.Add(legCarbonValues.ToArray());
            return legCarbonValues.Sum();
        }

        public double CalculateLegCarbonSurcharge(int quantity, double weightKg, double distanceKm, double storageCo2, TransportMode transportMode)
        {
            LegSurchargeRequests.Add((quantity, weightKg, distanceKm, storageCo2, transportMode));

            var legCarbon = (quantity * weightKg * distanceKm) + storageCo2;
            var surchargeRate = transportMode switch
            {
                TransportMode.PLANE => 0.12d,
                TransportMode.SHIP => 0.03d,
                TransportMode.TRAIN => 0.04d,
                _ => 0.05d
            };

            return legCarbon * surchargeRate;
        }

        public double CalculateTotalCarbonSurcharge(IReadOnlyList<double> legSurcharges)
        {
            TotalSurchargeRequests.Add(legSurcharges.ToArray());
            return legSurcharges.Sum();
        }

        public RouteQuoteResult CalculateRouteQuote(DeliveryRoute route, RouteQuoteInput quoteInput)
        {
            RouteQuoteRequests.Add((route, quoteInput));
            return new RouteQuoteResult(18m + RouteQuoteRequests.Count, 4.2d + RouteQuoteRequests.Count);
        }
    }

    private sealed class Phase4StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

        public Phase4StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
        {
            _responder = responder;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_responder(request));
        }
    }

    private sealed class Phase4RecordingGoogleMapsApi : IGoogleMapsAPI
    {
        public List<(string Origin, string Destination)> Requests { get; } = [];

        public Task<double> FetchRouteDistanceKmAsync(string origin, string destination, CancellationToken cancellationToken = default)
        {
            Requests.Add((origin, destination));
            throw new InvalidOperationException("Google route lookup should not have been called.");
        }
    }

    // Pricing-rule stub used so Feature 1 can assemble checkout options without relying on EF.
    private sealed class StubPricingRuleGateway : IPricingRuleGateway
    {
        public List<TransportMode> Requests { get; } = [];

        public List<PricingRule> FindActiveRules()
        {
            return [CreateRule(TransportMode.TRAIN, 1.0m, 0.05m)];
        }

        public List<PricingRule> FindByTransportMode(TransportMode mode)
        {
            Requests.Add(mode);

            return
            [
                mode switch
                {
                    TransportMode.PLANE => CreateRule(mode, 2.0m, 0.12m),
                    TransportMode.SHIP => CreateRule(mode, 0.6m, 0.03m),
                    TransportMode.TRAIN => CreateRule(mode, 0.9m, 0.04m),
                    _ => CreateRule(mode, 1.0m, 0.05m)
                }
            ];
        }

        public void Save(PricingRule rule)
        {
        }

        public void Update(PricingRule rule)
        {
        }

        private static PricingRule CreateRule(TransportMode transportMode, decimal baseRatePerKm, decimal surchargeRate)
        {
            var rule = new PricingRule();
            SetPrivateField(rule, "_transportMode", transportMode);
            SetPrivateField(rule, "_baseRatePerKm", baseRatePerKm);
            SetPrivateField(rule, "_carbonSurcharge", surchargeRate);
            SetPrivateField(rule, "_isActive", true);
            return rule;
        }
    }
}

// Phase 5 keeps the MVC boundary thin by testing controller coordination separately from the domain layer.
internal static class Phase5Tests
{
    public static IReadOnlyList<PhaseTest> All { get; } =
    [
        new("GetShippingOptions returns the preference card model and checkout id", GetShippingOptionsReturnsPreferenceCards),
        new("SelectShippingPreference returns the confirmed selection result", SelectShippingPreferenceReturnsSelectionResult),
        new("SelectShippingPreference re-renders preference cards when route resolution fails", SelectShippingPreferenceRerendersOptionsWhenRouteResolutionFails)
    ];

    private static void GetShippingOptionsReturnsPreferenceCards()
    {
        var options = CreateControllerOptions();
        var service = new ControllerShippingOptionService(options, CreateSelectionResult());
        var controller = new ShippingOptionsController(service);

        var result = controller.GetShippingOptions(42, CancellationToken.None).GetAwaiter().GetResult();
        var viewResult = AssertViewResult(result);
        var model = AssertModel<IReadOnlyList<ShippingPreferenceCard>>(viewResult);

        TestAssertions.AssertEqual(42, viewResult.ViewData["CheckoutId"]);
        TestAssertions.AssertEqual(3, model.Count);
        TestAssertions.AssertEqual(42, service.LastGetCheckoutId);
    }

    private static void SelectShippingPreferenceReturnsSelectionResult()
    {
        var expectedSelection = CreateSelectionResult();
        var service = new ControllerShippingOptionService(CreateControllerOptions(), expectedSelection);
        var controller = new ShippingOptionsController(service);

        var result = controller.SelectShippingPreference(42, PreferenceType.GREEN, CancellationToken.None).GetAwaiter().GetResult();
        var viewResult = AssertViewResult(result);
        var model = AssertModel<ShippingSelectionResult>(viewResult);

        TestAssertions.AssertEqual(42, service.LastSelectionRequest?.CheckoutId);
        TestAssertions.AssertEqual(PreferenceType.GREEN, service.LastSelectionRequest?.PreferenceType);
        TestAssertions.AssertEqual(expectedSelection, model);
    }

    private static void SelectShippingPreferenceRerendersOptionsWhenRouteResolutionFails()
    {
        var options = CreateControllerOptions();
        var service = new ControllerShippingOptionService(
            options,
            CreateSelectionResult(),
            new RouteResolutionException("At least two distinct airport addresses are required for PLANE route generation."));
        var controller = new ShippingOptionsController(service);

        var result = controller.SelectShippingPreference(42, PreferenceType.FAST, CancellationToken.None).GetAwaiter().GetResult();
        var viewResult = AssertViewResult(result);
        var model = AssertModel<IReadOnlyList<ShippingPreferenceCard>>(viewResult);

        TestAssertions.AssertEqual(42, service.LastSelectionRequest?.CheckoutId);
        TestAssertions.AssertEqual(PreferenceType.FAST, service.LastSelectionRequest?.PreferenceType);
        TestAssertions.AssertEqual(42, service.LastGetCheckoutId);
        TestAssertions.AssertEqual(42, viewResult.ViewData["CheckoutId"]);
        TestAssertions.AssertTrue(
            string.Equals(viewResult.ViewName, "~/Views/Module3/P2-1/ShippingOptions/Index.cshtml", StringComparison.Ordinal),
            "Expected the controller to re-render the shipping options page.");
        TestAssertions.AssertEqual(options.Count, model.Count);
        TestAssertions.AssertTrue(controller.ModelState.ErrorCount > 0, "Expected route-resolution failures to be added to ModelState.");
    }

    private static ViewResult AssertViewResult(IActionResult actionResult)
    {
        return actionResult as ViewResult
            ?? throw new InvalidOperationException($"Expected ViewResult but got {actionResult.GetType().Name}.");
    }

    private static T AssertModel<T>(ViewResult viewResult)
    {
        return viewResult.Model is T model
            ? model
            : throw new InvalidOperationException($"Expected model of type {typeof(T).Name}.");
    }

    private static IReadOnlyList<ShippingPreferenceCard> CreateControllerOptions() =>
    [
        new ShippingPreferenceCard(42, PreferenceType.FAST, "Fastest", "Fast profile", "PLANE"),
        new ShippingPreferenceCard(42, PreferenceType.CHEAP, "Cheapest", "Cheap profile", "SHIP"),
        new ShippingPreferenceCard(42, PreferenceType.GREEN, "Greenest", "Green profile", "TRAIN")
    ];

    private static ShippingSelectionResult CreateSelectionResult() =>
        new(42, 7, PreferenceType.GREEN, 18m, 4.4, 4, "TRAIN");

    // Controller-facing service double that captures input parameters and returns fixed view models.
    private sealed class ControllerShippingOptionService : IShippingOptionService
    {
        private readonly IReadOnlyList<ShippingPreferenceCard> _options;
        private readonly ShippingSelectionResult _selectionResult;
        private readonly Exception? _confirmException;

        public ControllerShippingOptionService(
            IReadOnlyList<ShippingPreferenceCard> options,
            ShippingSelectionResult selectionResult,
            Exception? confirmException = null)
        {
            _options = options;
            _selectionResult = selectionResult;
            _confirmException = confirmException;
        }

        public int? LastGetCheckoutId { get; private set; }
        public SelectShippingPreferenceRequest? LastSelectionRequest { get; private set; }

        public Task<IReadOnlyList<ShippingPreferenceCard>> GetPreferenceChoicesForCheckoutAsync(int checkoutId, CancellationToken cancellationToken = default)
        {
            LastGetCheckoutId = checkoutId;
            return Task.FromResult(_options);
        }

        public Task<ShippingSelectionResult> ConfirmPreferenceSelectionAsync(SelectShippingPreferenceRequest request, CancellationToken cancellationToken = default)
        {
            LastSelectionRequest = request;
            if (_confirmException is not null)
            {
                throw _confirmException;
            }

            return Task.FromResult(_selectionResult);
        }
    }
}

// Phase 6 checks container wiring and the temporary home-page entry point into Feature 1.
internal static class Phase6Tests
{
    public static IReadOnlyList<PhaseTest> All { get; } =
    [
        new("Feature1 DI registration resolves the shipping options stack", Feature1RegistrationResolvesExpectedServices),
        new("Google Maps startup warning is omitted when API key is configured", GoogleMapsStartupWarningIsOmittedWhenApiKeyIsConfigured),
        new("Google Maps startup warning is returned when API key is missing", GoogleMapsStartupWarningIsReturnedWhenApiKeyIsMissing),
        new("Home page exposes a temporary shipping options entry form", HomePageExposesShippingOptionsEntryForm)
    ];

    private static void Feature1RegistrationResolvesExpectedServices()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["GoogleMaps:ApiKey"] = string.Empty
            })
            .Build();

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql("Host=localhost;Database=testrental;Username=test;Password=test"));
        services.AddSingleton<IConfiguration>(configuration);
        services.AddScoped<IHubCarbonService, StubHubCarbonService>();
        services.AddFeature1Services();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var scopedProvider = scope.ServiceProvider;

        TestAssertions.AssertTrue(scopedProvider.GetService<IShippingOptionMapper>() is ShippingOptionMapper, "Expected shipping option mapper registration.");
        TestAssertions.AssertTrue(scopedProvider.GetService<ICheckoutShippingContextService>() is ShippingCheckoutContextService, "Expected checkout context service registration.");
        TestAssertions.AssertTrue(scopedProvider.GetService<IRoutingService>() is RouteManager, "Expected routing service registration.");
        TestAssertions.AssertTrue(scopedProvider.GetService<IGoogleMapsAPI>() is GoogleMapsAPI, "Expected Google Maps API registration.");
        TestAssertions.AssertTrue(scopedProvider.GetService<ITransportCarbonService>() is ProRental.Domain.Module3.P2_1.Controls.TransportCarbonManager, "Expected transport carbon service registration.");
        TestAssertions.AssertTrue(scopedProvider.GetService<IShippingPreferenceService>() is PreferenceManager, "Expected preference manager registration.");
        TestAssertions.AssertTrue(scopedProvider.GetService<IShippingOptionService>() is ShippingOptionManager, "Expected shipping option manager registration.");
        var strategies = scopedProvider.GetServices<IShippingPreferenceStrategy>().Select(strategy => strategy.PreferenceType).OrderBy(value => value).ToArray();
        TestAssertions.AssertSequence(new[] { PreferenceType.FAST, PreferenceType.CHEAP, PreferenceType.GREEN }.OrderBy(value => value), strategies);
    }

    private static void GoogleMapsStartupWarningIsOmittedWhenApiKeyIsConfigured()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [GoogleMapsConfigurationDiagnostics.ApiKeyConfigPath] = "test-key"
            })
            .Build();

        var warning = GoogleMapsConfigurationDiagnostics.GetMissingApiKeyWarning(configuration);

        TestAssertions.AssertNull(warning);
    }

    private static void GoogleMapsStartupWarningIsReturnedWhenApiKeyIsMissing()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [GoogleMapsConfigurationDiagnostics.ApiKeyConfigPath] = string.Empty
            })
            .Build();

        var warning = GoogleMapsConfigurationDiagnostics.GetMissingApiKeyWarning(configuration);

        TestAssertions.AssertTrue(
            !string.IsNullOrWhiteSpace(warning),
            "Expected a startup warning when the Google Maps API key is missing.");
        TestAssertions.AssertTrue(
            warning!.Contains("GoogleMaps__ApiKey", StringComparison.Ordinal),
            "Expected the startup warning to point to the environment variable name.");
        TestAssertions.AssertTrue(
            warning.Contains("GoogleMaps:ApiKey", StringComparison.Ordinal),
            "Expected the startup warning to point to the user-secrets key name.");
    }

    private static void HomePageExposesShippingOptionsEntryForm()
    {
        var homeIndexPath = Path.Combine(Directory.GetCurrentDirectory(), "Views", "Home", "Index.cshtml");
        var content = File.ReadAllText(homeIndexPath);

        TestAssertions.AssertTrue(content.Contains("asp-controller=\"ShippingOptions\"", StringComparison.Ordinal), "Expected home page to target ShippingOptionsController.");
        TestAssertions.AssertTrue(content.Contains("asp-action=\"GetShippingOptions\"", StringComparison.Ordinal), "Expected home page to post into GetShippingOptions.");
        TestAssertions.AssertTrue(content.Contains("name=\"checkoutId\"", StringComparison.Ordinal), "Expected home page to collect a checkoutId.");
    }
}

// Phase 7 runs the full Feature 1 flow against a real database fixture.
internal static class Phase7Tests
{
    public static IReadOnlyList<PhaseTest> All { get; } =
    [
        new("Feature1 shows three preference cards without persisting shipping options", Feature1ShowsPreferenceCardsWithoutPersistingOptions),
        new("Feature1 FAST selection falls back to direct TRAIN for same-country checkouts", Feature1FastSelectionFallsBackToTrainWhenCheckoutIsSameCountry),
        new("Feature1 CHEAP selection falls back to direct TRUCK for same-country checkouts", Feature1CheapSelectionFallsBackToTruckWhenCheckoutIsSameCountry),
        new("Feature1 selection writes one persisted option and checkout.option_id end-to-end for multi-item checkouts", Feature1AppliesSelectionEndToEnd),
        new("Feature1 GREEN selection falls back to SHIP when direct Google ground routing fails", Feature1GreenSelectionFallsBackToShipWhenTrainRouteUnavailable),
        new("RouteManager builds PLANE routes using warehouse-country and destination-country airports", RouteManagerBuildsPlaneRouteWithDistinctAirports),
        new("RouteManager builds SHIP routes using warehouse-country and destination-country ports", RouteManagerBuildsShipRouteWithDistinctPorts),
        new("RouteManager builds direct TRAIN routes from warehouse to customer", RouteManagerBuildsDirectTrainRoute),
        new("RouteManager falls back from TRAIN to SHIP when Google returns no direct ground route", RouteManagerFallsBackFromTrainToShipWhenGoogleReturnsNoDirectGroundRoute),
        new("RouteManager does not fall back from TRAIN to SHIP when Google API key is missing", RouteManagerDoesNotFallbackFromTrainToShipWhenGoogleApiKeyIsMissing),
        new("RouteManager fallback to SHIP fails cleanly without a destination-country port", RouteManagerFallbackToShipFailsCleanlyWithoutDistinctPorts),
        new("RouteManager rejects PLANE routes without distinct same-country airports", RouteManagerRejectsPlaneRouteWithoutDistinctAirports),
        new("RouteManager rejects SHIP routes without distinct same-country ports", RouteManagerRejectsShipRouteWithoutDistinctPorts),
        new("RouteManager rejects PLANE routes when the destination country is unsupported", RouteManagerRejectsPlaneRouteWhenDestinationCountryIsUnsupported),
        new("Feature1 checkout context aggregates real multi-item cart inputs", Feature1CheckoutContextResolvesRealInputs),
        new("Feature1 checkout context rejects checkouts without selected items", Feature1CheckoutContextRejectsCheckoutsWithoutSelectedItems),
        new("Feature1 confirm flow rolls back route and option writes when checkout selection fails", Feature1SelectionRollsBackOnFailure)
    ];

    private static void Feature1ShowsPreferenceCardsWithoutPersistingOptions()
    {
        using var context = Phase3Tests.CreateDbContext();
        using var transaction = context.Database.BeginTransaction();

        var snapshot = Phase3Tests.CreateCheckoutFixture(context);
        var manager = CreateManager(
            context,
            transportationHubMapper: new StubTransportationHubMapper(
            [
                CreateWarehouseHub(snapshot.WarehouseHubId, Phase3Tests.WarehouseAddress)
            ]));

        var options = manager.GetPreferenceChoicesForCheckoutAsync(snapshot.CheckoutId).GetAwaiter().GetResult();
        var persistedOptions = new ShippingOptionMapper(context).FindByCheckoutIdAsync(snapshot.CheckoutId).GetAwaiter().GetResult();

        TestAssertions.AssertEqual(3, options.Count);
        TestAssertions.AssertSequence(new[] { PreferenceType.FAST, PreferenceType.CHEAP, PreferenceType.GREEN }, options.Select(option => option.PreferenceType));
        TestAssertions.AssertEqual("TRAIN", options.Single(option => option.PreferenceType == PreferenceType.FAST).AllowedModesLabel);
        TestAssertions.AssertEqual("TRUCK", options.Single(option => option.PreferenceType == PreferenceType.CHEAP).AllowedModesLabel);
        TestAssertions.AssertEqual("TRAIN", options.Single(option => option.PreferenceType == PreferenceType.GREEN).AllowedModesLabel);
        TestAssertions.AssertEqual(0, persistedOptions.Count);

        transaction.Rollback();
    }

    private static void Feature1FastSelectionFallsBackToTrainWhenCheckoutIsSameCountry()
    {
        using var context = Phase3Tests.CreateDbContext();
        using var transaction = context.Database.BeginTransaction();

        var snapshot = Phase3Tests.CreateCheckoutFixture(context, includeAdditionalOrderItem: true);
        var manager = CreateManager(
            context,
            transportationHubMapper: new StubTransportationHubMapper(
            [
                CreateWarehouseHub(snapshot.WarehouseHubId, Phase3Tests.WarehouseAddress)
            ]));

        var result = manager.ConfirmPreferenceSelectionAsync(
            new SelectShippingPreferenceRequest(snapshot.CheckoutId, PreferenceType.FAST)).GetAwaiter().GetResult();

        context.ChangeTracker.Clear();
        var persistedOption = new ShippingOptionMapper(context).FindByCheckoutIdAsync(snapshot.CheckoutId).GetAwaiter().GetResult().Single();
        var routeId = persistedOption.GetSummary().RouteId
            ?? throw new InvalidOperationException("Expected persisted option to reference a route.");
        var route = context.DeliveryRoutes
            .Include(entity => entity.RouteLegs)
            .First(entity => EF.Property<int>(entity, "RouteId") == routeId);
        var routeLegs = route.GetOrderedRouteLegs();

        TestAssertions.AssertEqual("TRAIN", result.TransportModeLabel);
        TestAssertions.AssertEqual(TransportMode.TRAIN, persistedOption.GetSummary().TransportMode);
        TestAssertions.AssertEqual(1, routeLegs.Count);
        TestAssertions.AssertEqual(TransportMode.TRAIN, routeLegs[0].GetTransportMode() ?? throw new InvalidOperationException("Expected TRAIN main leg."));

        transaction.Rollback();
    }

    private static void Feature1CheapSelectionFallsBackToTruckWhenCheckoutIsSameCountry()
    {
        using var context = Phase3Tests.CreateDbContext();
        using var transaction = context.Database.BeginTransaction();

        var snapshot = Phase3Tests.CreateCheckoutFixture(context, includeAdditionalOrderItem: true);
        var manager = CreateManager(
            context,
            transportationHubMapper: new StubTransportationHubMapper(
            [
                CreateWarehouseHub(snapshot.WarehouseHubId, Phase3Tests.WarehouseAddress)
            ]));

        var result = manager.ConfirmPreferenceSelectionAsync(
            new SelectShippingPreferenceRequest(snapshot.CheckoutId, PreferenceType.CHEAP)).GetAwaiter().GetResult();

        context.ChangeTracker.Clear();
        var persistedOption = new ShippingOptionMapper(context).FindByCheckoutIdAsync(snapshot.CheckoutId).GetAwaiter().GetResult().Single();
        var routeId = persistedOption.GetSummary().RouteId
            ?? throw new InvalidOperationException("Expected persisted option to reference a route.");
        var route = context.DeliveryRoutes
            .Include(entity => entity.RouteLegs)
            .First(entity => EF.Property<int>(entity, "RouteId") == routeId);
        var routeLegs = route.GetOrderedRouteLegs();

        TestAssertions.AssertEqual("TRUCK", result.TransportModeLabel);
        TestAssertions.AssertEqual(TransportMode.TRUCK, persistedOption.GetSummary().TransportMode);
        TestAssertions.AssertEqual(1, routeLegs.Count);
        TestAssertions.AssertEqual(TransportMode.TRUCK, routeLegs[0].GetTransportMode() ?? throw new InvalidOperationException("Expected TRUCK main leg."));

        transaction.Rollback();
    }

    private static void Feature1AppliesSelectionEndToEnd()
    {
        using var context = Phase3Tests.CreateDbContext();
        using var transaction = context.Database.BeginTransaction();

        var snapshot = Phase3Tests.CreateCheckoutFixture(context, includeAdditionalOrderItem: true);
        var manager = CreateManager(
            context,
            transportationHubMapper: new StubTransportationHubMapper(
            [
                CreateWarehouseHub(snapshot.WarehouseHubId, Phase3Tests.WarehouseAddress)
            ]));

        var result = manager.ConfirmPreferenceSelectionAsync(
            new SelectShippingPreferenceRequest(snapshot.CheckoutId, PreferenceType.GREEN)).GetAwaiter().GetResult();

        context.ChangeTracker.Clear();
        var persistedOptions = new ShippingOptionMapper(context).FindByCheckoutIdAsync(snapshot.CheckoutId).GetAwaiter().GetResult();

        var checkout = context.Checkouts
            .First(entity => EF.Property<int>(entity, "Checkoutid") == snapshot.CheckoutId);
        var persistedOption = persistedOptions.Single();
        var routeId = persistedOption.GetSummary().RouteId
            ?? throw new InvalidOperationException("Expected persisted option to reference a route.");
        var route = context.DeliveryRoutes
            .Include(entity => entity.RouteLegs)
            .First(entity => EF.Property<int>(entity, "RouteId") == routeId);
        var routeLegs = route.GetOrderedRouteLegs();
        var resolvedWarehouseAddress = route.GetOriginAddress();

        TestAssertions.AssertEqual(1, persistedOptions.Count);
        TestAssertions.AssertEqual(result.OptionId, checkout.GetSelectionState().SelectedOptionId);
        TestAssertions.AssertEqual(PreferenceType.GREEN, result.PreferenceType);
        TestAssertions.AssertTrue(result.OptionId > 0, "Expected a persisted shipping option id.");
        TestAssertions.AssertEqual("TRAIN", result.TransportModeLabel);
        TestAssertions.AssertEqual(1, routeLegs.Count);
        TestAssertions.AssertTrue(routeLegs[0].GetIsMainTransport() == true, "Expected the TRAIN leg to be marked as the main transport leg.");
        TestAssertions.AssertEqual(TransportMode.TRAIN, routeLegs[0].GetTransportMode() ?? throw new InvalidOperationException("Expected main leg transport mode."));
        TestAssertions.AssertEqual(resolvedWarehouseAddress, routeLegs[0].GetStartPoint());
        TestAssertions.AssertEqual(Phase3Tests.CustomerAddress, routeLegs[0].GetEndPoint());

        transaction.Rollback();
    }

    private static void Feature1GreenSelectionFallsBackToShipWhenTrainRouteUnavailable()
    {
        using var context = Phase3Tests.CreateDbContext();
        using var transaction = context.Database.BeginTransaction();

        var snapshot = Phase3Tests.CreateCheckoutFixture(
            context,
            includeAdditionalOrderItem: true,
            customerAddress: Phase3Tests.UnitedStatesCustomerAddress);
        var routeIdsBefore = context.DeliveryRoutes
            .Select(entity => EF.Property<int>(entity, "RouteId"))
            .ToArray();
        var warehouseAddress = new TransportationHubMapper(context).FindByType(HubType.WAREHOUSE)
            .Select(hub => hub.GetAddress())
            .First(address => !string.IsNullOrWhiteSpace(address));
        var manager = new ShippingOptionManager(
            new ShippingOptionMapper(context),
            new ShippingCheckoutContextService(context, new StubInventoryService(), new TransportationHubMapper(context)),
            Phase4Tests.CreatePreferenceService(),
            CreateRouteManager(
                context,
                new StubTransportationHubMapper(
                [
                    CreateWarehouseHub(1, warehouseAddress),
                    CreateShippingPortHub(2, Phase3Tests.ShippingPortAddressOne, "SGP", "Singapore Port", latitude: 0d, longitude: 0d),
                    CreateShippingPortHub(3, Phase3Tests.UnitedStatesShippingPortAddress, "LAX", "Port of Los Angeles", latitude: 1d, longitude: 0d)
                ]),
                new StubGoogleMapsApi(
                new Dictionary<(string Origin, string Destination), double>
                {
                    [(warehouseAddress, Phase3Tests.ShippingPortAddressOne)] = 11d,
                    [(Phase3Tests.UnitedStatesShippingPortAddress, Phase3Tests.UnitedStatesCustomerAddress)] = 9d
                },
                new Dictionary<(string Origin, string Destination), Exception>
                {
                    [(warehouseAddress, Phase3Tests.UnitedStatesCustomerAddress)] =
                        new RouteResolutionException($"Google Maps did not return a route distance for '{warehouseAddress}' to '{Phase3Tests.UnitedStatesCustomerAddress}'.")
                })),
            new TransportationHubMapper(context),
            new ProRental.Domain.Module3.P2_1.Controls.TransportCarbonManager(new PricingRuleGateway(context), new StubHubCarbonService()),
            context);

        var result = manager.ConfirmPreferenceSelectionAsync(
            new SelectShippingPreferenceRequest(snapshot.CheckoutId, PreferenceType.GREEN)).GetAwaiter().GetResult();

        context.ChangeTracker.Clear();
        var persistedOptions = new ShippingOptionMapper(context).FindByCheckoutIdAsync(snapshot.CheckoutId).GetAwaiter().GetResult();
        var persistedOption = persistedOptions.Single();
        var routeId = persistedOption.GetSummary().RouteId
            ?? throw new InvalidOperationException("Expected persisted option to reference a route.");
        var route = context.DeliveryRoutes
            .Include(entity => entity.RouteLegs)
            .First(entity => EF.Property<int>(entity, "RouteId") == routeId);
        var routeLegs = route.GetOrderedRouteLegs();
        var newRouteIds = context.DeliveryRoutes
            .Select(entity => EF.Property<int>(entity, "RouteId"))
            .Where(id => !routeIdsBefore.Contains(id))
            .ToArray();

        TestAssertions.AssertEqual(PreferenceType.GREEN, result.PreferenceType);
        TestAssertions.AssertEqual("SHIP", result.TransportModeLabel);
        TestAssertions.AssertEqual(TransportMode.SHIP, persistedOption.GetSummary().TransportMode);
        TestAssertions.AssertEqual(3, routeLegs.Count);
        TestAssertions.AssertEqual(TransportMode.SHIP, routeLegs[1].GetTransportMode() ?? throw new InvalidOperationException("Expected SHIP main leg."));
        TestAssertions.AssertEqual(131.2d, route.GetTotalDistanceKm());
        TestAssertions.AssertEqual(1, newRouteIds.Length);

        transaction.Rollback();
    }

    private static void RouteManagerBuildsPlaneRouteWithDistinctAirports()
    {
        using var context = Phase3Tests.CreateDbContext();
        using var transaction = context.Database.BeginTransaction();
        var googleMapsApi = new StubGoogleMapsApi(new Dictionary<(string Origin, string Destination), double>
        {
            [(Phase3Tests.WarehouseAddress, Phase3Tests.AirportAddressOne)] = 18d,
            [(Phase3Tests.AirportAddressTwo, Phase3Tests.JapanCustomerAddress)] = 14d
        });

        var routeManager = CreateRouteManager(
            context,
            new StubTransportationHubMapper(
            [
                CreateWarehouseHub(1, Phase3Tests.WarehouseAddress),
                CreateAirportHub(2, Phase3Tests.AirportAddressOne, "SIN", "Singapore Changi Airport", latitude: 0d, longitude: 1d),
                CreateAirportHub(3, Phase3Tests.AirportAddressTwo, "NRT", "Narita International Airport", latitude: 0d, longitude: 0d)
            ]),
            googleMapsApi);

        var route = routeManager.CreateMultiModalRouteAsync("ProRental Warehouse", Phase3Tests.JapanCustomerAddress, [TransportMode.PLANE]).GetAwaiter().GetResult();
        var routeLegs = route.GetOrderedRouteLegs();

        TestAssertions.AssertEqual(3, routeLegs.Count);
        TestAssertions.AssertEqual(Phase3Tests.WarehouseAddress, routeLegs[0].GetStartPoint());
        TestAssertions.AssertEqual(Phase3Tests.AirportAddressOne, routeLegs[0].GetEndPoint());
        TestAssertions.AssertEqual(Phase3Tests.AirportAddressOne, routeLegs[1].GetStartPoint());
        TestAssertions.AssertEqual(Phase3Tests.AirportAddressTwo, routeLegs[1].GetEndPoint());
        TestAssertions.AssertEqual(Phase3Tests.AirportAddressTwo, routeLegs[2].GetStartPoint());
        TestAssertions.AssertEqual(Phase3Tests.JapanCustomerAddress, routeLegs[2].GetEndPoint());
        TestAssertions.AssertEqual(143.2d, route.GetTotalDistanceKm());
        TestAssertions.AssertSequence(
            new[]
            {
                (Phase3Tests.WarehouseAddress, Phase3Tests.AirportAddressOne),
                (Phase3Tests.AirportAddressTwo, Phase3Tests.JapanCustomerAddress)
            },
            googleMapsApi.Requests);

        transaction.Rollback();
    }

    private static void RouteManagerBuildsShipRouteWithDistinctPorts()
    {
        using var context = Phase3Tests.CreateDbContext();
        using var transaction = context.Database.BeginTransaction();
        var googleMapsApi = new StubGoogleMapsApi(new Dictionary<(string Origin, string Destination), double>
        {
            [(Phase3Tests.WarehouseAddress, Phase3Tests.ShippingPortAddressOne)] = 11d,
            [(Phase3Tests.UnitedStatesShippingPortAddress, Phase3Tests.UnitedStatesCustomerAddress)] = 9d
        });

        var routeManager = CreateRouteManager(
            context,
            new StubTransportationHubMapper(
            [
                CreateWarehouseHub(1, Phase3Tests.WarehouseAddress),
                CreateShippingPortHub(2, Phase3Tests.ShippingPortAddressOne, "SGP", "Singapore Port", latitude: 0d, longitude: 0d),
                CreateShippingPortHub(3, Phase3Tests.UnitedStatesShippingPortAddress, "LAX", "Port of Los Angeles", latitude: 1d, longitude: 0d)
            ]),
            googleMapsApi);

        var route = routeManager.CreateMultiModalRouteAsync("ProRental Warehouse", Phase3Tests.UnitedStatesCustomerAddress, [TransportMode.SHIP]).GetAwaiter().GetResult();
        var routeLegs = route.GetOrderedRouteLegs();

        TestAssertions.AssertEqual(3, routeLegs.Count);
        TestAssertions.AssertEqual(Phase3Tests.WarehouseAddress, routeLegs[0].GetStartPoint());
        TestAssertions.AssertEqual(Phase3Tests.ShippingPortAddressOne, routeLegs[0].GetEndPoint());
        TestAssertions.AssertEqual(Phase3Tests.ShippingPortAddressOne, routeLegs[1].GetStartPoint());
        TestAssertions.AssertEqual(Phase3Tests.UnitedStatesShippingPortAddress, routeLegs[1].GetEndPoint());
        TestAssertions.AssertEqual(Phase3Tests.UnitedStatesShippingPortAddress, routeLegs[2].GetStartPoint());
        TestAssertions.AssertEqual(Phase3Tests.UnitedStatesCustomerAddress, routeLegs[2].GetEndPoint());
        TestAssertions.AssertEqual(131.2d, route.GetTotalDistanceKm());
        TestAssertions.AssertSequence(
            new[]
            {
                (Phase3Tests.WarehouseAddress, Phase3Tests.ShippingPortAddressOne),
                (Phase3Tests.UnitedStatesShippingPortAddress, Phase3Tests.UnitedStatesCustomerAddress)
            },
            googleMapsApi.Requests);

        transaction.Rollback();
    }

    private static void RouteManagerBuildsDirectTrainRoute()
    {
        using var context = Phase3Tests.CreateDbContext();
        using var transaction = context.Database.BeginTransaction();

        var routeManager = CreateRouteManager(
            context,
            new StubTransportationHubMapper([CreateWarehouseHub(1, Phase3Tests.WarehouseAddress)]),
            new StubGoogleMapsApi(new Dictionary<(string Origin, string Destination), double>
            {
                [(Phase3Tests.WarehouseAddress, Phase3Tests.CustomerAddress)] = 24d
            }));

        var route = routeManager.CreateMultiModalRouteAsync("ProRental Warehouse", Phase3Tests.CustomerAddress, [TransportMode.TRAIN]).GetAwaiter().GetResult();
        var routeLegs = route.GetOrderedRouteLegs();

        TestAssertions.AssertEqual(1, routeLegs.Count);
        TestAssertions.AssertEqual(Phase3Tests.WarehouseAddress, routeLegs[0].GetStartPoint());
        TestAssertions.AssertEqual(Phase3Tests.CustomerAddress, routeLegs[0].GetEndPoint());
        TestAssertions.AssertTrue(routeLegs[0].GetIsMainTransport() == true, "Expected the TRAIN leg to be marked as main transport.");
        TestAssertions.AssertEqual(24d, route.GetTotalDistanceKm());

        transaction.Rollback();
    }

    private static void RouteManagerFallsBackFromTrainToShipWhenGoogleReturnsNoDirectGroundRoute()
    {
        using var context = Phase3Tests.CreateDbContext();
        using var transaction = context.Database.BeginTransaction();

        var routeManager = CreateRouteManager(
            context,
            new StubTransportationHubMapper(
            [
                CreateWarehouseHub(1, Phase3Tests.WarehouseAddress),
                CreateShippingPortHub(2, Phase3Tests.ShippingPortAddressOne, "SGP", "Singapore Port", latitude: 0d, longitude: 0d),
                CreateShippingPortHub(3, Phase3Tests.UnitedStatesShippingPortAddress, "LAX", "Port of Los Angeles", latitude: 1d, longitude: 0d)
            ]),
            new StubGoogleMapsApi(
                new Dictionary<(string Origin, string Destination), double>
                {
                    [(Phase3Tests.WarehouseAddress, Phase3Tests.ShippingPortAddressOne)] = 11d,
                    [(Phase3Tests.UnitedStatesShippingPortAddress, Phase3Tests.UnitedStatesCustomerAddress)] = 9d
                },
                new Dictionary<(string Origin, string Destination), Exception>
                {
                    [(Phase3Tests.WarehouseAddress, Phase3Tests.UnitedStatesCustomerAddress)] =
                        new RouteResolutionException($"Google Maps did not return a route distance for '{Phase3Tests.WarehouseAddress}' to '{Phase3Tests.UnitedStatesCustomerAddress}'.")
                }));

        var route = routeManager.CreateMultiModalRouteAsync("ProRental Warehouse", Phase3Tests.UnitedStatesCustomerAddress, [TransportMode.TRAIN, TransportMode.SHIP]).GetAwaiter().GetResult();
        var routeLegs = route.GetOrderedRouteLegs();

        TestAssertions.AssertEqual(3, routeLegs.Count);
        TestAssertions.AssertEqual(TransportMode.SHIP, routeLegs[1].GetTransportMode() ?? throw new InvalidOperationException("Expected SHIP main leg."));
        TestAssertions.AssertEqual(131.2d, route.GetTotalDistanceKm());

        transaction.Rollback();
    }

    private static void RouteManagerDoesNotFallbackFromTrainToShipWhenGoogleApiKeyIsMissing()
    {
        using var context = Phase3Tests.CreateDbContext();
        using var transaction = context.Database.BeginTransaction();

        var routeCountBefore = context.DeliveryRoutes.Count();
        var routeManager = CreateRouteManager(
            context,
            new StubTransportationHubMapper(
            [
                CreateWarehouseHub(1, Phase3Tests.WarehouseAddress),
                CreateShippingPortHub(2, Phase3Tests.ShippingPortAddressOne, "SGP", "Singapore Port", latitude: 0d, longitude: 0d),
                CreateShippingPortHub(3, Phase3Tests.UnitedStatesShippingPortAddress, "LAX", "Port of Los Angeles", latitude: 1d, longitude: 0d)
            ]),
            CreateGoogleMapsApi(_ => throw new InvalidOperationException("Google should not be called without an API key."), apiKey: string.Empty));

        var exception = TestAssertions.AssertThrows<RouteResolutionException>(
            () => routeManager.CreateMultiModalRouteAsync("ProRental Warehouse", Phase3Tests.UnitedStatesCustomerAddress, [TransportMode.TRAIN, TransportMode.SHIP]).GetAwaiter().GetResult(),
            "Expected TRAIN->SHIP fallback not to occur when the Google API key is missing.");

        TestAssertions.AssertTrue(
            exception.Message.Contains("configured API key", StringComparison.Ordinal),
            "Expected the missing API key error to be surfaced.");
        TestAssertions.AssertEqual(routeCountBefore, context.DeliveryRoutes.Count());

        transaction.Rollback();
    }

    private static void RouteManagerFallbackToShipFailsCleanlyWithoutDistinctPorts()
    {
        using var context = Phase3Tests.CreateDbContext();
        using var transaction = context.Database.BeginTransaction();

        var routeCountBefore = context.DeliveryRoutes.Count();
        var routeManager = CreateRouteManager(
            context,
            new StubTransportationHubMapper(
            [
                CreateWarehouseHub(1, Phase3Tests.WarehouseAddress),
                CreateShippingPortHub(2, Phase3Tests.ShippingPortAddressOne, "SGP", "Singapore Port", latitude: 0d, longitude: 0d)
            ]),
            new StubGoogleMapsApi(
                distances: null,
                exceptions: new Dictionary<(string Origin, string Destination), Exception>
                {
                    [(Phase3Tests.WarehouseAddress, Phase3Tests.UnitedStatesCustomerAddress)] =
                        new RouteResolutionException($"Google Maps did not return a route distance for '{Phase3Tests.WarehouseAddress}' to '{Phase3Tests.UnitedStatesCustomerAddress}'.")
                }));

        var exception = TestAssertions.AssertThrows<RouteResolutionException>(
            () => routeManager.CreateMultiModalRouteAsync("ProRental Warehouse", Phase3Tests.UnitedStatesCustomerAddress, [TransportMode.TRAIN, TransportMode.SHIP]).GetAwaiter().GetResult(),
            "Expected GREEN fallback to fail when a destination-country port is unavailable.");

        TestAssertions.AssertTrue(
            exception.Message.Contains("customer destination country 'US'", StringComparison.Ordinal),
            "Expected the missing destination-country port error after TRAIN fallback.");
        TestAssertions.AssertEqual(routeCountBefore, context.DeliveryRoutes.Count());

        transaction.Rollback();
    }

    private static void RouteManagerRejectsPlaneRouteWithoutDistinctAirports()
    {
        using var context = Phase3Tests.CreateDbContext();
        using var transaction = context.Database.BeginTransaction();

        var routeCountBefore = context.DeliveryRoutes.Count();
        var routeManager = CreateRouteManager(
            context,
            new StubTransportationHubMapper(
            [
                CreateWarehouseHub(1, Phase3Tests.WarehouseAddress),
                CreateAirportHub(2, Phase3Tests.AirportAddressOne, "SIN", "Singapore Changi Airport")
            ]),
            new StubGoogleMapsApi());

        var exception = TestAssertions.AssertThrows<RouteResolutionException>(
            () => routeManager.CreateMultiModalRouteAsync("ProRental Warehouse", Phase3Tests.CustomerAddress, [TransportMode.PLANE]).GetAwaiter().GetResult(),
            "Expected PLANE route generation to fail without a second airport.");

        TestAssertions.AssertTrue(
            exception.Message.Contains("Distinct airport addresses", StringComparison.Ordinal),
            "Expected a missing-airport error message.");
        TestAssertions.AssertEqual(routeCountBefore, context.DeliveryRoutes.Count());

        transaction.Rollback();
    }

    private static void RouteManagerRejectsShipRouteWithoutDistinctPorts()
    {
        using var context = Phase3Tests.CreateDbContext();
        using var transaction = context.Database.BeginTransaction();

        var routeCountBefore = context.DeliveryRoutes.Count();
        var routeManager = CreateRouteManager(
            context,
            new StubTransportationHubMapper(
            [
                CreateWarehouseHub(1, Phase3Tests.WarehouseAddress),
                CreateShippingPortHub(2, Phase3Tests.ShippingPortAddressOne, "SGP", "Singapore Port")
            ]),
            new StubGoogleMapsApi());

        var exception = TestAssertions.AssertThrows<RouteResolutionException>(
            () => routeManager.CreateMultiModalRouteAsync("ProRental Warehouse", Phase3Tests.CustomerAddress, [TransportMode.SHIP]).GetAwaiter().GetResult(),
            "Expected SHIP route generation to fail without a second shipping port.");

        TestAssertions.AssertTrue(
            exception.Message.Contains("Distinct shipping port addresses", StringComparison.Ordinal),
            "Expected a missing-port error message.");
        TestAssertions.AssertEqual(routeCountBefore, context.DeliveryRoutes.Count());

        transaction.Rollback();
    }

    private static void RouteManagerRejectsPlaneRouteWhenDestinationCountryIsUnsupported()
    {
        using var context = Phase3Tests.CreateDbContext();
        using var transaction = context.Database.BeginTransaction();

        var routeCountBefore = context.DeliveryRoutes.Count();
        var routeManager = CreateRouteManager(
            context,
            new StubTransportationHubMapper(
            [
                CreateWarehouseHub(1, Phase3Tests.WarehouseAddress),
                CreateAirportHub(2, Phase3Tests.AirportAddressOne, "SIN", "Singapore Changi Airport"),
                CreateAirportHub(3, Phase3Tests.AirportAddressTwo, "NRT", "Narita International Airport")
            ]),
            new StubGoogleMapsApi());

        var exception = TestAssertions.AssertThrows<RouteResolutionException>(
            () => routeManager.CreateMultiModalRouteAsync("ProRental Warehouse", "123 Example Road", [TransportMode.PLANE]).GetAwaiter().GetResult(),
            "Expected PLANE route generation to fail when the destination country cannot be parsed.");

        TestAssertions.AssertTrue(
            exception.Message.Contains("supported country name or country code", StringComparison.Ordinal),
            "Expected an unsupported-country parsing error message.");
        TestAssertions.AssertEqual(routeCountBefore, context.DeliveryRoutes.Count());

        transaction.Rollback();
    }

    private static void Feature1CheckoutContextResolvesRealInputs()
    {
        using var context = Phase3Tests.CreateDbContext();
        using var transaction = context.Database.BeginTransaction();

        var snapshot = Phase3Tests.CreateCheckoutFixture(context, includeAdditionalOrderItem: true);
        Phase3Tests.CreateCartItemFixture(context, snapshot.CartId, snapshot.ProductId, quantity: 1);

        var service = new ShippingCheckoutContextService(
            context,
            new StubInventoryService(),
            new TransportationHubMapper(context));

        var shippingContext = service.GetShippingContextAsync(snapshot.CheckoutId).GetAwaiter().GetResult()
            ?? throw new InvalidOperationException("Expected a shipping context result.");
        var resolvedWarehouse = new TransportationHubMapper(context).FindById(shippingContext.HubId);
        var firstProduct = shippingContext.Items.Single(item => item.ProductId == snapshot.ProductIds[0]);
        var secondProduct = shippingContext.Items.Single(item => item.ProductId == snapshot.ProductIds[1]);

        TestAssertions.AssertEqual(snapshot.CheckoutId, shippingContext.CheckoutId);
        TestAssertions.AssertTrue(shippingContext.HubId > 0, "Expected a resolved warehouse hub id.");
        TestAssertions.AssertTrue(resolvedWarehouse?.GetHubType() == HubType.WAREHOUSE, "Expected the resolved hub to be a warehouse.");
        TestAssertions.AssertEqual(2, shippingContext.Items.Count);
        TestAssertions.AssertEqual(3, firstProduct.Quantity);
        TestAssertions.AssertEqual(1, secondProduct.Quantity);
        TestAssertions.AssertEqual(5.5d, firstProduct.UnitWeightKg);
        TestAssertions.AssertEqual(5.5d, secondProduct.UnitWeightKg);
        TestAssertions.AssertEqual(22d, shippingContext.TotalShipmentWeightKg);

        transaction.Rollback();
    }

    private static void Feature1CheckoutContextRejectsCheckoutsWithoutSelectedItems()
    {
        using (var context = Phase3Tests.CreateDbContext())
        using (var transaction = context.Database.BeginTransaction())
        {
            var snapshot = Phase3Tests.CreateCheckoutFixture(context);
            context.Cartitems
                .Where(entity => EF.Property<int>(entity, "Cartid") == snapshot.CartId)
                .ExecuteDelete();

            var service = new ShippingCheckoutContextService(
                context,
                new StubInventoryService(),
                new TransportationHubMapper(context));

            var missingItemsException = TestAssertions.AssertThrows<InvalidOperationException>(
                () => service.GetShippingContextAsync(snapshot.CheckoutId).GetAwaiter().GetResult(),
                "Expected checkout-context lookup to reject checkouts without selected items.");
            TestAssertions.AssertTrue(
                missingItemsException.Message.Contains("does not contain any selected cart items", StringComparison.Ordinal),
                "Expected missing-items error message.");

            transaction.Rollback();
        }
    }

    private static void Feature1SelectionRollsBackOnFailure()
    {
        using var context = Phase3Tests.CreateDbContext();

        var snapshot = Phase3Tests.CreateCheckoutFixture(context, includeAdditionalOrderItem: true);
        var routeIdsBefore = context.DeliveryRoutes
            .Select(entity => EF.Property<int>(entity, "RouteId"))
            .ToArray();

        try
        {
            var manager = CreateManager(context, new ThrowingCheckoutSelectionMapper(context));

            TestAssertions.AssertThrows<InvalidOperationException>(
                () => manager.ConfirmPreferenceSelectionAsync(
                    new SelectShippingPreferenceRequest(snapshot.CheckoutId, PreferenceType.GREEN)).GetAwaiter().GetResult(),
                "Expected the confirm flow to surface the simulated checkout failure.");

            context.ChangeTracker.Clear();

            var persistedOptions = new ShippingOptionMapper(context).FindByCheckoutIdAsync(snapshot.CheckoutId).GetAwaiter().GetResult();
            var routeIdsAfter = context.DeliveryRoutes
                .Select(entity => EF.Property<int>(entity, "RouteId"))
                .ToArray();

            TestAssertions.AssertEqual(0, persistedOptions.Count);
            TestAssertions.AssertSequence(routeIdsBefore.OrderBy(id => id), routeIdsAfter.OrderBy(id => id));
        }
        finally
        {
            CleanupCheckoutFixture(context, snapshot, routeIdsBefore);
        }
    }

    private static ShippingOptionManager CreateManager(
        AppDbContext context,
        IShippingOptionMapper? mapper = null,
        IGoogleMapsAPI? googleMapsApi = null,
        IEnumerable<TransportationHub>? additionalHubs = null,
        ITransportationHubMapper? transportationHubMapper = null)
    {
        var hubMapper = transportationHubMapper ?? new TransportationHubMapper(context);
        var warehouseAddress = hubMapper.FindByType(HubType.WAREHOUSE)
            .Select(hub => hub.GetAddress())
            .First(address => !string.IsNullOrWhiteSpace(address));
        if (additionalHubs is not null)
        {
            foreach (var hub in additionalHubs)
            {
                hub.SetHubId(0);
            }

            context.TransportationHubs.AddRange(additionalHubs);
            context.SaveChanges();
            hubMapper = new TransportationHubMapper(context);
            warehouseAddress = hubMapper.FindByType(HubType.WAREHOUSE)
                .Select(hub => hub.GetAddress())
                .First(address => !string.IsNullOrWhiteSpace(address));
        }

        return new ShippingOptionManager(
            mapper ?? new ShippingOptionMapper(context),
            new ShippingCheckoutContextService(context, new StubInventoryService(), hubMapper),
            Phase4Tests.CreatePreferenceService(),
            CreateRouteManager(context, hubMapper, googleMapsApi ?? new StubGoogleMapsApi(new Dictionary<(string Origin, string Destination), double>
            {
                [(warehouseAddress, Phase3Tests.CustomerAddress)] = 24d
            })),
            hubMapper,
            new ProRental.Domain.Module3.P2_1.Controls.TransportCarbonManager(new PricingRuleGateway(context), new StubHubCarbonService()),
            context);
    }

    private static RouteManager CreateRouteManager(
        AppDbContext context,
        ITransportationHubMapper hubMapper,
        IGoogleMapsAPI googleMapsApi)
    {
        var routeDistanceCalculator = new RouteDistanceCalculator(googleMapsApi);
        return new RouteManager(
            new RouteMapper(context),
            hubMapper,
            new RouteLegBuilder(routeDistanceCalculator),
            routeDistanceCalculator);
    }

    private static void CleanupCheckoutFixture(AppDbContext context, Phase3Tests.CheckoutFixture snapshot, IReadOnlyCollection<int> routeIdsBefore)
    {
        context.ChangeTracker.Clear();

        var newRouteIds = context.DeliveryRoutes
            .Select(entity => EF.Property<int>(entity, "RouteId"))
            .Where(routeId => !routeIdsBefore.Contains(routeId))
            .ToArray();

        context.ShippingOptions
            .Where(entity => EF.Property<int>(entity, "CheckoutId") == snapshot.CheckoutId)
            .ExecuteDelete();

        if (newRouteIds.Length > 0)
        {
            context.RouteLegs
                .Where(entity => newRouteIds.Contains(EF.Property<int>(entity, "RouteId")))
                .ExecuteDelete();
            context.DeliveryRoutes
                .Where(entity => newRouteIds.Contains(EF.Property<int>(entity, "RouteId")))
                .ExecuteDelete();
        }

        context.Cartitems
            .Where(entity => EF.Property<int>(entity, "Cartid") == snapshot.CartId)
            .ExecuteDelete();
        context.Checkouts
            .Where(entity => EF.Property<int>(entity, "Checkoutid") == snapshot.CheckoutId)
            .ExecuteDelete();
        context.Carts
            .Where(entity => EF.Property<int>(entity, "Cartid") == snapshot.CartId)
            .ExecuteDelete();
        context.Products
            .Where(entity => snapshot.ProductIds.Contains(EF.Property<int>(entity, "Productid")))
            .ExecuteDelete();
        context.Categories
            .Where(entity => EF.Property<int>(entity, "Categoryid") == snapshot.CategoryId)
            .ExecuteDelete();

        var warehouse = context.Set<Warehouse>()
            .FirstOrDefault(entity => EF.Property<int>(entity, "HubId") == snapshot.WarehouseHubId);
        if (warehouse is not null)
        {
            context.Remove(warehouse);
            context.SaveChanges();
        }

        context.Customers
            .Where(entity => EF.Property<int>(entity, "Customerid") == snapshot.CustomerId)
            .ExecuteDelete();
        context.Users
            .Where(entity => EF.Property<int>(entity, "Userid") == snapshot.UserId)
            .ExecuteDelete();
    }

    private sealed class StubInventoryService : IInventoryService
    {
        public Product? GetProductById(int productId) => null;
        public decimal GetProductWeight(int productId) => 5.5m;
        public List<InventoryProductDropdownItem> GetAllProductDropdownItems() => [];
        public ProductStorageInfo? GetProductStorageInfo(int productId) => null;
        public List<ProductStorageInfo> GetAllProductStorageInfo() => [];
    }

    private static GoogleMapsAPI CreateGoogleMapsApi(Func<HttpRequestMessage, HttpResponseMessage> responder, string apiKey = "test-key")
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["GoogleMaps:ApiKey"] = apiKey
            })
            .Build();

        var httpClient = new HttpClient(new StubHttpMessageHandler(responder))
        {
            BaseAddress = new Uri("https://routes.googleapis.com/")
        };

        return new GoogleMapsAPI(httpClient, configuration);
    }

    private static Warehouse CreateWarehouseHub(int hubId, string address)
    {
        var warehouse = new Warehouse();
        warehouse.SetHubId(hubId);
        warehouse.SetHubType(HubType.WAREHOUSE);
        warehouse.SetAddress(address);
        warehouse.SetCountryCode("SG");
        warehouse.SetLatitude(1.290270);
        warehouse.SetLongitude(103.851959);
        warehouse.SetWarehouseCode($"WH-{hubId}");
        warehouse.SetMaxProductCapacity(100);
        warehouse.SetTotalWarehouseVolume(1000d);
        return warehouse;
    }

    private static Airport CreateAirportHub(int hubId, string address, string airportCode, string airportName, double latitude = 1.0, double longitude = 103.0)
    {
        var airport = new Airport();
        airport.SetHubId(hubId);
        airport.SetHubType(HubType.AIRPORT);
        airport.SetAddress(address);
        airport.SetCountryCode(ResolveCountryCodeFromAddress(address));
        airport.SetLatitude(latitude);
        airport.SetLongitude(longitude);
        airport.SetAirportCode(airportCode);
        airport.SetAirportName(airportName);
        return airport;
    }

    private static ShippingPort CreateShippingPortHub(int hubId, string address, string portCode, string portName, double latitude = 1.0, double longitude = 103.0)
    {
        var port = new ShippingPort();
        port.SetHubId(hubId);
        port.SetHubType(HubType.SHIPPING_PORT);
        port.SetAddress(address);
        port.SetCountryCode(ResolveCountryCodeFromAddress(address));
        port.SetLatitude(latitude);
        port.SetLongitude(longitude);
        port.SetPortCode(portCode);
        port.SetPortName(portName);
        port.SetPortType("CONTAINER");
        return port;
    }

    private static string ResolveCountryCodeFromAddress(string address)
    {
        if (address.Contains("Singapore", StringComparison.OrdinalIgnoreCase))
        {
            return "SG";
        }

        if (address.Contains("Japan", StringComparison.OrdinalIgnoreCase))
        {
            return "JP";
        }

        if (address.Contains("USA", StringComparison.OrdinalIgnoreCase) ||
            address.Contains("United States", StringComparison.OrdinalIgnoreCase))
        {
            return "US";
        }

        if (address.Contains("Philippines", StringComparison.OrdinalIgnoreCase))
        {
            return "PH";
        }

        throw new InvalidOperationException($"Unsupported test country mapping for address '{address}'.");
    }

    private sealed class StubGoogleMapsApi : IGoogleMapsAPI
    {
        private readonly IReadOnlyDictionary<(string Origin, string Destination), double> _distances;
        private readonly IReadOnlyDictionary<(string Origin, string Destination), Exception> _exceptions;

        public StubGoogleMapsApi(
            IReadOnlyDictionary<(string Origin, string Destination), double>? distances = null,
            IReadOnlyDictionary<(string Origin, string Destination), Exception>? exceptions = null)
        {
            _distances = distances ?? new Dictionary<(string Origin, string Destination), double>();
            _exceptions = exceptions ?? new Dictionary<(string Origin, string Destination), Exception>();
        }

        public List<(string Origin, string Destination)> Requests { get; } = [];

        public Task<double> FetchRouteDistanceKmAsync(string origin, string destination, CancellationToken cancellationToken = default)
        {
            Requests.Add((origin, destination));
            if (_exceptions.TryGetValue((origin, destination), out var exception))
            {
                throw exception;
            }

            if (_distances.TryGetValue((origin, destination), out var distanceKm))
            {
                return Task.FromResult(distanceKm);
            }

            throw new RouteResolutionException($"No stubbed Google Maps distance was configured for '{origin}' to '{destination}'.");
        }
    }

    private sealed class StubTransportationHubMapper : ITransportationHubMapper
    {
        private readonly List<TransportationHub> _hubs;

        public StubTransportationHubMapper(IEnumerable<TransportationHub> hubs)
        {
            _hubs = hubs.ToList();
        }

        public TransportationHub? FindById(int hubId) =>
            _hubs.FirstOrDefault(hub => hub.GetHubId() == hubId);

        public List<TransportationHub> FindByType(HubType hubType) =>
            _hubs.Where(hub => hub.GetHubType() == hubType).ToList();

        public List<TransportationHub> FindAll() => _hubs.ToList();

        public void Insert(TransportationHub hub) => _hubs.Add(hub);

        public void Update(TransportationHub hub)
        {
            var index = _hubs.FindIndex(existingHub => existingHub.GetHubId() == hub.GetHubId());
            if (index >= 0)
            {
                _hubs[index] = hub;
            }
        }

        public void Delete(int hubId)
        {
            _hubs.RemoveAll(hub => hub.GetHubId() == hubId);
        }
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

        public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
        {
            _responder = responder;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_responder(request));
        }
    }

    private sealed class ThrowingCheckoutSelectionMapper : IShippingOptionMapper
    {
        private readonly ShippingOptionMapper _inner;

        public ThrowingCheckoutSelectionMapper(AppDbContext context)
        {
            _inner = new ShippingOptionMapper(context);
        }

        public Task<Checkout?> FindCheckoutAsync(int checkoutId, CancellationToken cancellationToken = default) =>
            _inner.FindCheckoutAsync(checkoutId, cancellationToken);

        public Task<IReadOnlyList<ShippingOption>> FindByCheckoutIdAsync(int checkoutId, CancellationToken cancellationToken = default) =>
            _inner.FindByCheckoutIdAsync(checkoutId, cancellationToken);

        public Task<ShippingOption?> FindByIdAsync(int optionId, CancellationToken cancellationToken = default) =>
            _inner.FindByIdAsync(optionId, cancellationToken);

        public Task<int?> FindSelectedRouteIdByOrderIdAsync(int orderId, CancellationToken cancellationToken = default) =>
            _inner.FindSelectedRouteIdByOrderIdAsync(orderId, cancellationToken);

        public Task AddAsync(ShippingOption option, CancellationToken cancellationToken = default) =>
            _inner.AddAsync(option, cancellationToken);

        public Task AddRangeAsync(IEnumerable<ShippingOption> options, CancellationToken cancellationToken = default) =>
            _inner.AddRangeAsync(options, cancellationToken);

        public Task UpdateAsync(ShippingOption option, CancellationToken cancellationToken = default) =>
            _inner.UpdateAsync(option, cancellationToken);

        public Task SetCheckoutSelectedOptionAsync(int checkoutId, int optionId, CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("Simulated checkout selection failure.");

        public Task SaveChangesAsync(CancellationToken cancellationToken = default) =>
            _inner.SaveChangesAsync(cancellationToken);
    }
}

internal static class TestAssertions
{
    public static void AssertEqual<T>(T expected, T actual)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new InvalidOperationException($"Expected '{expected}' but got '{actual}'.");
        }
    }

    public static void AssertNull(object? value)
    {
        if (value is not null)
        {
            throw new InvalidOperationException($"Expected null but got '{value}'.");
        }
    }

    public static void AssertSequence<T>(IEnumerable<T> expected, IEnumerable<T> actual)
    {
        var expectedItems = expected.ToArray();
        var actualItems = actual.ToArray();

        if (!expectedItems.SequenceEqual(actualItems))
        {
            throw new InvalidOperationException(
                $"Expected sequence [{string.Join(", ", expectedItems)}] but got [{string.Join(", ", actualItems)}].");
        }
    }

    public static void AssertTrue(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    public static TException AssertThrows<TException>(Action action, string message)
        where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException exception)
        {
            return exception;
        }

        throw new InvalidOperationException(message);
    }
}
