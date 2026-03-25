using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using ProRental.Configuration.Module3.P2_1;
using ProRental.Controllers;
using ProRental.Data.Gateways;
using ProRental.Data.UnitOfWork;
using ProRental.Data.Module3.P2_1;
using ProRental.Data.Module3.P2_1.Gateways;
using ProRental.Data.Module3.P2_1.Interfaces;
using ProRental.Domain.Controls;
using ProRental.Domain.Entities;
using ProRental.Domain.Enums;
using ProRental.Interfaces;
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
        new("RouteLeg derives exactly one role flag", RouteLegDerivesExactlyOneRoleFlag),
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
        TestAssertions.AssertEqual(11, summary.OrderId);
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

    private static void RouteLegDerivesExactlyOneRoleFlag()
    {
        var firstLeg = new RouteLeg();
        firstLeg.ConfigureLeg(1, "Warehouse", "Airport Hub", 18d, TransportMode.PLANE, true, false);

        TestAssertions.AssertEqual(true, firstLeg.GetIsFirstMile());
        TestAssertions.AssertEqual(false, firstLeg.GetIsMainTransport());
        TestAssertions.AssertEqual(false, firstLeg.GetIsLastMile());

        var lastLeg = new RouteLeg();
        lastLeg.ConfigureLeg(2, "Airport Hub", "Customer", 8d, TransportMode.TRUCK, false, true);

        TestAssertions.AssertEqual(false, lastLeg.GetIsFirstMile());
        TestAssertions.AssertEqual(false, lastLeg.GetIsMainTransport());
        TestAssertions.AssertEqual(true, lastLeg.GetIsLastMile());

        var mainLeg = new RouteLeg();
        mainLeg.ConfigureLeg(2, "Airport Hub", "Rail Hub", 12d, TransportMode.TRAIN, false, true, false);

        TestAssertions.AssertEqual(false, mainLeg.GetIsFirstMile());
        TestAssertions.AssertEqual(true, mainLeg.GetIsMainTransport());
        TestAssertions.AssertEqual(false, mainLeg.GetIsLastMile());

        var singleLeg = new RouteLeg();
        singleLeg.ConfigureLeg(1, "Warehouse", "Customer", 24d, TransportMode.TRUCK, true, true);

        TestAssertions.AssertEqual(false, singleLeg.GetIsFirstMile());
        TestAssertions.AssertEqual(true, singleLeg.GetIsMainTransport());
        TestAssertions.AssertEqual(false, singleLeg.GetIsLastMile());
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
        new("Ranking contract is keyed to shipping-option summary models", RankingContractsUseShippingOptionSummary),
        new("Repository and dependency contracts expose the next-phase entry points", RepositoryAndDependencyContractsArePresent)
    ];

    private static void ShippingOptionSummaryCarriesFullCheckoutPayload()
    {
        var summary = new ShippingOptionSummary(
            OptionId: 1,
            OrderId: 2,
            PreferenceType: PreferenceType.FAST,
            DisplayName: "Fastest",
            Cost: 25.00m,
            CarbonFootprintKg: 14.2,
            DeliveryDays: 2,
            RouteId: 8,
            TransportMode: TransportMode.PLANE,
            TransportModeLabel: "Plane + Truck");

        TestAssertions.AssertEqual(1, summary.OptionId);
        TestAssertions.AssertEqual(2, summary.OrderId);
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

        AssertMethod(contract, "GetPreferenceChoicesForOrderAsync", typeof(Task<IReadOnlyList<ShippingPreferenceCard>>), typeof(int), typeof(CancellationToken));
        AssertMethod(contract, "ConfirmPreferenceSelectionAsync", typeof(Task<ShippingSelectionResult>), typeof(SelectShippingPreferenceRequest), typeof(CancellationToken));
    }

    private static void RankingContractsUseShippingOptionSummary()
    {
        var rankingService = typeof(IRankingService);
        var rankingStrategy = typeof(IRankingStrategy);

        AssertMethod(rankingService, "RankBySpeed", typeof(IReadOnlyList<ShippingOptionSummary>), typeof(IEnumerable<ShippingOptionSummary>));
        AssertMethod(rankingService, "RankByCost", typeof(IReadOnlyList<ShippingOptionSummary>), typeof(IEnumerable<ShippingOptionSummary>));
        AssertMethod(rankingService, "RankByCarbon", typeof(IReadOnlyList<ShippingOptionSummary>), typeof(IEnumerable<ShippingOptionSummary>));

        var preferenceProperty = rankingStrategy.GetProperty(nameof(IRankingStrategy.PreferenceType))
            ?? throw new InvalidOperationException("IRankingStrategy.PreferenceType was not found.");
        TestAssertions.AssertEqual(typeof(PreferenceType), preferenceProperty.PropertyType);

        AssertMethod(rankingStrategy, "Rank", typeof(IReadOnlyList<ShippingOptionSummary>), typeof(IEnumerable<ShippingOptionSummary>));
    }

    private static void RepositoryAndDependencyContractsArePresent()
    {
        var mapper = typeof(IShippingOptionMapper);
        var orderService = typeof(IOrderService);
        var routingService = typeof(IRoutingService);
        var transportCarbonService = typeof(ITransportCarbonService);
        var orderShippingContext = typeof(OrderShippingContext);

        AssertMethod(mapper, "FindOrderWithCheckoutAsync", typeof(Task<Order?>), typeof(int), typeof(CancellationToken));
        AssertMethod(mapper, "FindByOrderIdAsync", typeof(Task<IReadOnlyList<ShippingOption>>), typeof(int), typeof(CancellationToken));
        AssertMethod(mapper, "FindByIdAsync", typeof(Task<ShippingOption?>), typeof(int), typeof(CancellationToken));
        AssertMethod(mapper, "AddAsync", typeof(Task), typeof(ShippingOption), typeof(CancellationToken));
        AssertMethod(mapper, "AddRangeAsync", typeof(Task), typeof(IEnumerable<ShippingOption>), typeof(CancellationToken));
        AssertMethod(mapper, "UpdateAsync", typeof(Task), typeof(ShippingOption), typeof(CancellationToken));
        AssertMethod(mapper, "SetCheckoutSelectedOptionAsync", typeof(Task), typeof(int), typeof(int), typeof(CancellationToken));
        AssertMethod(mapper, "SaveChangesAsync", typeof(Task), typeof(CancellationToken));

        AssertMethod(orderService, "GetShippingContextAsync", typeof(Task<OrderShippingContext?>), typeof(int), typeof(CancellationToken));
        AssertMethod(routingService, "CreateMultiModalRoute", typeof(DeliveryRoute), typeof(string), typeof(string), typeof(List<TransportMode>));
        AssertMethod(transportCarbonService, "CalculateLegCarbon", typeof(double), typeof(int), typeof(double), typeof(double), typeof(double));
        AssertMethod(transportCarbonService, "CalculateRouteCarbon", typeof(double), typeof(IReadOnlyList<double>));
        AssertMethod(transportCarbonService, "CalculateLegCarbonSurcharge", typeof(double), typeof(int), typeof(double), typeof(double), typeof(double), typeof(TransportMode));
        AssertMethod(transportCarbonService, "CalculateTotalCarbonSurcharge", typeof(double), typeof(IReadOnlyList<double>));
        AssertMethod(transportCarbonService, "CalculateRouteQuote", typeof(RouteQuoteResult), typeof(DeliveryRoute), typeof(RouteQuoteInput));

        TestAssertions.AssertEqual(typeof(IReadOnlyList<OrderShippingItem>), orderShippingContext.GetProperty(nameof(OrderShippingContext.Items))?.PropertyType);
        TestAssertions.AssertEqual(typeof(double), orderShippingContext.GetProperty(nameof(OrderShippingContext.TotalShipmentWeightKg))?.PropertyType);
    }

    private static void AssertMethod(Type type, string name, Type returnType, params Type[] parameterTypes)
    {
        var method = type.GetMethod(name, parameterTypes)
            ?? throw new InvalidOperationException($"{type.Name}.{name} was not found.");

        TestAssertions.AssertEqual(returnType, method.ReturnType);
    }
}

// Phase 2 covers the ranking subsystem in isolation so strategy behavior stays deterministic.
internal static class Phase2Tests
{
    public static IReadOnlyList<PhaseTest> All { get; } =
    [
        new("Fastest strategy ranks by delivery days with deterministic tie-breakers", FastestStrategyUsesDeterministicOrdering),
        new("Cheapest strategy ranks by cost with deterministic tie-breakers", CheapestStrategyUsesDeterministicOrdering),
        new("Eco-friendly strategy ranks by carbon with deterministic tie-breakers", EcoFriendlyStrategyUsesDeterministicOrdering),
        new("Ranking manager routes each criterion to the matching strategy", RankingManagerUsesRegisteredStrategies),
        new("Ranking manager rejects missing strategies", RankingManagerRejectsMissingStrategies)
    ];

    private static void FastestStrategyUsesDeterministicOrdering()
    {
        var strategy = new FastestStrategy();
        var ranked = strategy.Rank(CreateSampleOptions());

        TestAssertions.AssertSequence(new[] { 4, 2, 1, 3 }, ranked.Select(option => option.OptionId));
    }

    private static void CheapestStrategyUsesDeterministicOrdering()
    {
        var strategy = new CheapestStrategy();
        var ranked = strategy.Rank(CreateSampleOptions());

        TestAssertions.AssertSequence(new[] { 4, 3, 2, 1 }, ranked.Select(option => option.OptionId));
    }

    private static void EcoFriendlyStrategyUsesDeterministicOrdering()
    {
        var strategy = new EcoFriendlyStrategy();
        var ranked = strategy.Rank(CreateSampleOptions());

        TestAssertions.AssertSequence(new[] { 4, 3, 1, 2 }, ranked.Select(option => option.OptionId));
    }

    private static void RankingManagerUsesRegisteredStrategies()
    {
        var manager = new RankingManager(
        [
            new FastestStrategy(),
            new CheapestStrategy(),
            new EcoFriendlyStrategy()
        ]);

        TestAssertions.AssertSequence(new[] { 4, 2, 1, 3 }, manager.RankBySpeed(CreateSampleOptions()).Select(option => option.OptionId));
        TestAssertions.AssertSequence(new[] { 4, 3, 2, 1 }, manager.RankByCost(CreateSampleOptions()).Select(option => option.OptionId));
        TestAssertions.AssertSequence(new[] { 4, 3, 1, 2 }, manager.RankByCarbon(CreateSampleOptions()).Select(option => option.OptionId));
    }

    private static void RankingManagerRejectsMissingStrategies()
    {
        var manager = new RankingManager([new FastestStrategy()]);

        try
        {
            _ = manager.RankByCarbon(CreateSampleOptions());
            throw new InvalidOperationException("Expected RankByCarbon to reject a missing GREEN strategy.");
        }
        catch (InvalidOperationException ex)
        {
            if (!ex.Message.Contains("GREEN", StringComparison.Ordinal))
            {
                throw;
            }
        }
    }

    private static IReadOnlyList<ShippingOptionSummary> CreateSampleOptions() =>
    [
        new ShippingOptionSummary(1, 10, PreferenceType.FAST, "Option 1", 20.00m, 9.0, 3, 100, TransportMode.TRUCK, "Truck"),
        new ShippingOptionSummary(2, 10, PreferenceType.CHEAP, "Option 2", 18.00m, 12.0, 2, 101, TransportMode.PLANE, "Plane"),
        new ShippingOptionSummary(3, 10, PreferenceType.GREEN, "Option 3", 15.00m, 8.0, 4, 102, TransportMode.SHIP, "Ship"),
        new ShippingOptionSummary(4, 10, PreferenceType.GREEN, "Option 4", 15.00m, 6.0, 2, 103, TransportMode.TRAIN, "Train")
    ];
}

// Phase 3 verifies the EF-backed mapper against a real PostgreSQL test fixture.
internal static class Phase3Tests
{
    public static IReadOnlyList<PhaseTest> All { get; } =
    [
        new("Repository can load an order with checkout state", RepositoryCanLoadOrderWithCheckout),
        new("Repository can insert and reload shipping options for an order", RepositoryCanInsertAndReloadShippingOption),
        new("Repository can persist checkout.option_id selection", RepositoryCanPersistCheckoutSelection)
    ];

    private static void RepositoryCanLoadOrderWithCheckout()
    {
        using var context = CreateDbContext();
        using var transaction = context.Database.BeginTransaction();

        var snapshot = CreateOrderFixture(context);
        var repository = new ShippingOptionMapper(context);

        var order = repository.FindOrderWithCheckoutAsync(snapshot.OrderId).GetAwaiter().GetResult()
            ?? throw new InvalidOperationException("Expected repository to return an order.");

        var orderContext = order.GetOrderContext();
        TestAssertions.AssertEqual(snapshot.OrderId, orderContext.OrderId);
        TestAssertions.AssertEqual(snapshot.CheckoutId, orderContext.CheckoutId);
        TestAssertions.AssertTrue(order.Checkout is not null, "Expected order checkout navigation to be loaded.");

        transaction.Rollback();
    }

    private static void RepositoryCanInsertAndReloadShippingOption()
    {
        using var context = CreateDbContext();
        using var transaction = context.Database.BeginTransaction();

        var snapshot = CreateOrderFixture(context);
        var repository = new ShippingOptionMapper(context);
        var route = CreateRoute(context);

        var option = new ShippingOption();
        option.ConfigureGeneratedOption(snapshot.OrderId, route.GetRouteId(), PreferenceType.GREEN, "Phase 3 Test Option", 31.25m, 7.5, 4, TransportMode.TRAIN);

        repository.AddAsync(option).GetAwaiter().GetResult();
        repository.SaveChangesAsync().GetAwaiter().GetResult();

        var insertedSummary = option.GetSummary();
        TestAssertions.AssertTrue(insertedSummary.OptionId > 0, "Expected inserted shipping option to receive an identity value.");

        context.ChangeTracker.Clear();

        var reloaded = repository.FindByIdAsync(insertedSummary.OptionId).GetAwaiter().GetResult()
            ?? throw new InvalidOperationException("Expected inserted shipping option to be queryable by id.");
        var orderOptions = repository.FindByOrderIdAsync(snapshot.OrderId).GetAwaiter().GetResult();
        var reloadedSummary = reloaded.GetSummary();

        TestAssertions.AssertEqual("Phase 3 Test Option", reloadedSummary.DisplayName);
        TestAssertions.AssertEqual(PreferenceType.GREEN, reloadedSummary.PreferenceType);
        TestAssertions.AssertTrue(orderOptions.Any(item => item.GetSummary().OptionId == insertedSummary.OptionId), "Expected inserted option to be returned by order lookup.");

        transaction.Rollback();
    }

    private static void RepositoryCanPersistCheckoutSelection()
    {
        using var context = CreateDbContext();
        using var transaction = context.Database.BeginTransaction();

        var snapshot = CreateOrderFixture(context);
        var repository = new ShippingOptionMapper(context);
        var route = CreateRoute(context);
        var option = CreateShippingOption(snapshot.OrderId, route.GetRouteId(), PreferenceType.CHEAP, TransportMode.TRUCK, "Selected Option");

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

    internal static OrderFixture CreateOrderFixture(AppDbContext context, bool includeAdditionalOrderItem = false)
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
        customer.SetAddress("Phase 3 Address");
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

        var order = new Order();
        order.InitializeForCheckout(
            customer.GetCustomerId(),
            checkout.GetSelectionState().CheckoutId,
            DateTime.UtcNow,
            120.00m);
        context.Orders.Add(order);
        context.SaveChanges();

        var warehouseHubId = CreateWarehouseFixture(context, suffix);
        var (categoryId, productId) = CreateProductFixture(context, suffix);
        var productIds = new List<int> { productId };
        CreateOrderItemFixture(context, order.GetOrderContext().OrderId, productId);

        if (includeAdditionalOrderItem)
        {
            var (_, secondProductId) = CreateProductFixture(context, $"{suffix}-2", categoryId);
            productIds.Add(secondProductId);
            CreateOrderItemFixture(context, order.GetOrderContext().OrderId, secondProductId, quantity: 1);
        }

        return new OrderFixture(
            order.GetOrderContext().OrderId,
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
        warehouse.SetAddress($"Phase 3 Warehouse {suffix}");
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

    internal static void CreateOrderItemFixture(AppDbContext context, int orderId, int productId, int quantity = 2)
    {
        var orderItem = new Orderitem();
        SetPrivateField(orderItem, "_orderid", orderId);
        SetPrivateField(orderItem, "_productid", productId);
        SetPrivateField(orderItem, "_quantity", quantity);
        SetPrivateField(orderItem, "_unitprice", 60.00m);
        SetPrivateField(orderItem, "_rentalstartdate", DateTime.UtcNow.Date);
        SetPrivateField(orderItem, "_rentalenddate", DateTime.UtcNow.Date.AddDays(3));

        context.Orderitems.Add(orderItem);
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
        int orderId,
        int routeId,
        PreferenceType preferenceType,
        TransportMode transportMode,
        string displayName)
    {
        var option = new ShippingOption();
        option.ConfigureGeneratedOption(orderId, routeId, preferenceType, displayName, 19.99m, 5.5, 3, transportMode);
        return option;
    }

    internal sealed record OrderFixture(
        int OrderId,
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
        new("ShippingOptionManager returns preference cards without routing or carbon calls", ShippingOptionManagerReturnsPreferenceCardsWithoutRouting),
        new("ShippingOptionManager confirms one preference with one route and one quote call", ShippingOptionManagerConfirmsPreferenceAndPersistsSingleOption),
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

    private static void ShippingOptionManagerReturnsPreferenceCardsWithoutRouting()
    {
        var repository = new InMemoryShippingOptionMapper();
        var orderService = new StubOrderService(CreateOrderShippingContext());
        var routingService = new StubRoutingService();
        var transportCarbonService = new StubTransportCarbonService();
        var manager = new ShippingOptionManager(repository, orderService, routingService, transportCarbonService);

        var result = manager.GetPreferenceChoicesForOrderAsync(orderService.Context.OrderId).GetAwaiter().GetResult();

        TestAssertions.AssertEqual(3, result.Count);
        TestAssertions.AssertSequence(
            new[] { PreferenceType.FAST, PreferenceType.CHEAP, PreferenceType.GREEN },
            result.Select(option => option.PreferenceType));
        TestAssertions.AssertEqual(0, repository.StoredOptions.Count);
        TestAssertions.AssertEqual(0, routingService.Requests.Count);
        TestAssertions.AssertEqual(0, transportCarbonService.RouteQuoteRequests.Count);
    }

    private static void ShippingOptionManagerConfirmsPreferenceAndPersistsSingleOption()
    {
        var repository = new InMemoryShippingOptionMapper();
        repository.Order = CreateOrder(10, 30, 20);
        var orderService = new StubOrderService(CreateOrderShippingContext());
        var routingService = new StubRoutingService();
        var transportCarbonService = new StubTransportCarbonService();
        var manager = new ShippingOptionManager(repository, orderService, routingService, transportCarbonService);

        var result = manager.ConfirmPreferenceSelectionAsync(
            new SelectShippingPreferenceRequest(10, PreferenceType.GREEN)).GetAwaiter().GetResult();

        TestAssertions.AssertEqual(30, repository.LastSelectedCheckoutId);
        TestAssertions.AssertEqual(1, repository.LastSelectedOptionId);
        TestAssertions.AssertEqual(1, repository.StoredOptions.Count);
        TestAssertions.AssertEqual(1, routingService.Requests.Count);
        TestAssertions.AssertEqual(1, transportCarbonService.RouteQuoteRequests.Count);
        TestAssertions.AssertEqual(PreferenceType.GREEN, result.PreferenceType);
        TestAssertions.AssertEqual("TRAIN", result.TransportModeLabel);

        var quoteRequest = transportCarbonService.RouteQuoteRequests.Single();
        TestAssertions.AssertEqual(202, quoteRequest.QuoteInput.HubId);
        TestAssertions.AssertEqual(2, quoteRequest.QuoteInput.Items.Count);
        TestAssertions.AssertEqual(16.5d, quoteRequest.QuoteInput.Items.Sum(item => item.Quantity * item.UnitWeightKg));
    }

    private static void ShippingOptionManagerReusesPersistedOptionOnReselection()
    {
        var repository = new InMemoryShippingOptionMapper();
        repository.Order = CreateOrder(10, 30, 20);
        repository.Seed(CreatePersistedOption(5, 10, PreferenceType.CHEAP, "Cheapest", 12m, 3.2, 5, TransportMode.SHIP));

        var routingService = new StubRoutingService();
        var transportCarbonService = new StubTransportCarbonService();
        var manager = new ShippingOptionManager(
            repository,
            new StubOrderService(CreateOrderShippingContext()),
            routingService,
            transportCarbonService);

        var selection = manager.ConfirmPreferenceSelectionAsync(
            new SelectShippingPreferenceRequest(10, PreferenceType.FAST)).GetAwaiter().GetResult();

        TestAssertions.AssertEqual(30, repository.LastSelectedCheckoutId);
        TestAssertions.AssertEqual(5, repository.LastSelectedOptionId);
        TestAssertions.AssertEqual(1, repository.StoredOptions.Count);
        TestAssertions.AssertEqual(1, routingService.Requests.Count);
        TestAssertions.AssertEqual(1, transportCarbonService.RouteQuoteRequests.Count);
        TestAssertions.AssertEqual(PreferenceType.FAST, selection.PreferenceType);
        TestAssertions.AssertEqual("PLANE", selection.TransportModeLabel);
    }

    private static OrderShippingContext CreateOrderShippingContext()
    {
        OrderShippingItem[] items =
        [
            new OrderShippingItem(101, 2, 5.5d),
            new OrderShippingItem(102, 1, 5.5d)
        ];

        return new OrderShippingContext(10, 20, 30, "Singapore", 202, items, 16.5d);
    }

    private static ShippingOption CreatePersistedOption(
        int optionId,
        int orderId,
        PreferenceType preferenceType,
        string displayName,
        decimal cost,
        double carbonFootprintKg,
        int deliveryDays,
        TransportMode transportMode)
    {
        var option = new ShippingOption();
        option.ConfigureGeneratedOption(orderId, null, preferenceType, displayName, cost, carbonFootprintKg, deliveryDays, transportMode);
        SetPrivateField(option, "_optionId", optionId);
        return option;
    }

    private static Order CreateOrder(int orderId, int checkoutId, int customerId)
    {
        var order = new Order();
        order.InitializeForCheckout(customerId, checkoutId, DateTime.UtcNow, 100m);
        SetPrivateField(order, "_orderid", orderId);
        return order;
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
        public Order? Order { get; set; }
        public int? LastSelectedCheckoutId { get; private set; }
        public int? LastSelectedOptionId { get; private set; }
        private int _nextOptionId = 1;

        public Task<Order?> FindOrderWithCheckoutAsync(int orderId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Order is not null && Order.GetOrderContext().OrderId == orderId ? Order : null);
        }

        public Task<IReadOnlyList<ShippingOption>> FindByOrderIdAsync(int orderId, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<ShippingOption> options = StoredOptions.Where(option => option.BelongsToOrder(orderId)).ToArray();
            return Task.FromResult(options);
        }

        public Task<ShippingOption?> FindByIdAsync(int optionId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(StoredOptions.FirstOrDefault(option => option.GetSummary().OptionId == optionId));
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
    private sealed class StubOrderService : IOrderService
    {
        public StubOrderService(OrderShippingContext context)
        {
            Context = context;
        }

        public OrderShippingContext Context { get; }
        public int CallCount { get; private set; }

        public Task<OrderShippingContext?> GetShippingContextAsync(int orderId, CancellationToken cancellationToken = default)
        {
            CallCount++;
            return Task.FromResult<OrderShippingContext?>(Context.OrderId == orderId ? Context : null);
        }
    }

    // Stub Feature 3 adapter that returns one deterministic route for a selected preference.
    private sealed class StubRoutingService : IRoutingService
    {
        public List<(string Origin, string Destination, IReadOnlyList<TransportMode> Modes)> Requests { get; } = [];

        public DeliveryRoute CreateMultiModalRoute(string origin, string destination, List<TransportMode> modes)
        {
            Requests.Add((origin, destination, modes.ToArray()));
            var route = new DeliveryRoute();
            route.SetOriginAddress(origin);
            route.SetDestinationAddress(destination);
            route.SetIsValid(true);
            route.SetTotalDistanceKm(24d);

            var primaryMode = modes.FirstOrDefault();
            var firstLeg = new RouteLeg();
            firstLeg.ConfigureLeg(1, origin, "Departure Hub", 4d, TransportMode.TRUCK, true, false, false);
            route.RouteLegs.Add(firstLeg);

            var mainLeg = new RouteLeg();
            mainLeg.ConfigureLeg(2, "Departure Hub", "Arrival Hub", 16d, primaryMode, false, true, false);
            route.RouteLegs.Add(mainLeg);

            var lastLeg = new RouteLeg();
            lastLeg.ConfigureLeg(3, "Arrival Hub", destination, 4d, TransportMode.TRUCK, false, false, true);
            route.RouteLegs.Add(lastLeg);

            return route;
        }
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
        new("GetShippingOptions returns the preference card model and order id", GetShippingOptionsReturnsPreferenceCards),
        new("SelectShippingPreference returns the confirmed selection result", SelectShippingPreferenceReturnsSelectionResult)
    ];

    private static void GetShippingOptionsReturnsPreferenceCards()
    {
        var options = CreateControllerOptions();
        var service = new ControllerShippingOptionService(options, CreateSelectionResult());
        var controller = new ShippingOptionsController(service);

        var result = controller.GetShippingOptions(42, CancellationToken.None).GetAwaiter().GetResult();
        var viewResult = AssertViewResult(result);
        var model = AssertModel<IReadOnlyList<ShippingPreferenceCard>>(viewResult);

        TestAssertions.AssertEqual(42, viewResult.ViewData["OrderId"]);
        TestAssertions.AssertEqual(3, model.Count);
        TestAssertions.AssertEqual(42, service.LastGetOrderId);
    }

    private static void SelectShippingPreferenceReturnsSelectionResult()
    {
        var expectedSelection = CreateSelectionResult();
        var service = new ControllerShippingOptionService(CreateControllerOptions(), expectedSelection);
        var controller = new ShippingOptionsController(service);

        var result = controller.SelectShippingPreference(42, PreferenceType.GREEN, CancellationToken.None).GetAwaiter().GetResult();
        var viewResult = AssertViewResult(result);
        var model = AssertModel<ShippingSelectionResult>(viewResult);

        TestAssertions.AssertEqual(42, service.LastSelectionRequest?.OrderId);
        TestAssertions.AssertEqual(PreferenceType.GREEN, service.LastSelectionRequest?.PreferenceType);
        TestAssertions.AssertEqual(expectedSelection, model);
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

        public ControllerShippingOptionService(
            IReadOnlyList<ShippingPreferenceCard> options,
            ShippingSelectionResult selectionResult)
        {
            _options = options;
            _selectionResult = selectionResult;
        }

        public int? LastGetOrderId { get; private set; }
        public SelectShippingPreferenceRequest? LastSelectionRequest { get; private set; }

        public Task<IReadOnlyList<ShippingPreferenceCard>> GetPreferenceChoicesForOrderAsync(int orderId, CancellationToken cancellationToken = default)
        {
            LastGetOrderId = orderId;
            return Task.FromResult(_options);
        }

        public Task<ShippingSelectionResult> ConfirmPreferenceSelectionAsync(SelectShippingPreferenceRequest request, CancellationToken cancellationToken = default)
        {
            LastSelectionRequest = request;
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
        TestAssertions.AssertTrue(scopedProvider.GetService<IOrderService>() is ShippingOrderContextService, "Expected order context service registration.");
        TestAssertions.AssertTrue(scopedProvider.GetService<IRoutingService>() is RouteManager, "Expected routing service registration.");
        TestAssertions.AssertTrue(scopedProvider.GetService<ITransportCarbonService>() is ProRental.Domain.Module3.P2_1.Controls.TransportCarbonManager, "Expected transport carbon service registration.");
        TestAssertions.AssertTrue(scopedProvider.GetService<IShippingOptionService>() is ShippingOptionManager, "Expected shipping option manager registration.");
        TestAssertions.AssertTrue(scopedProvider.GetService<IRankingService>() is RankingManager, "Expected ranking manager registration.");

        var strategies = scopedProvider.GetServices<IRankingStrategy>().Select(strategy => strategy.PreferenceType).OrderBy(value => value).ToArray();
        TestAssertions.AssertSequence(new[] { PreferenceType.FAST, PreferenceType.CHEAP, PreferenceType.GREEN }.OrderBy(value => value), strategies);
    }

    private static void HomePageExposesShippingOptionsEntryForm()
    {
        var homeIndexPath = Path.Combine(Directory.GetCurrentDirectory(), "Views", "Home", "Index.cshtml");
        var content = File.ReadAllText(homeIndexPath);

        TestAssertions.AssertTrue(content.Contains("asp-controller=\"ShippingOptions\"", StringComparison.Ordinal), "Expected home page to target ShippingOptionsController.");
        TestAssertions.AssertTrue(content.Contains("asp-action=\"GetShippingOptions\"", StringComparison.Ordinal), "Expected home page to post into GetShippingOptions.");
        TestAssertions.AssertTrue(content.Contains("name=\"orderId\"", StringComparison.Ordinal), "Expected home page to collect an orderId.");
    }
}

// Phase 7 runs the full Feature 1 flow against a real database fixture.
internal static class Phase7Tests
{
    public static IReadOnlyList<PhaseTest> All { get; } =
    [
        new("Feature1 shows three preference cards without persisting shipping options", Feature1ShowsPreferenceCardsWithoutPersistingOptions),
        new("Feature1 selection writes one persisted option and checkout.option_id end-to-end for multi-item orders", Feature1AppliesSelectionEndToEnd),
        new("Feature1 order context aggregates real multi-item order inputs", Feature1OrderContextResolvesRealInputs),
        new("Feature1 order context rejects orders without items", Feature1OrderContextRejectsOrdersWithoutItems),
        new("Feature1 confirm flow rolls back route and option writes when checkout selection fails", Feature1SelectionRollsBackOnFailure)
    ];

    private static void Feature1ShowsPreferenceCardsWithoutPersistingOptions()
    {
        using var context = Phase3Tests.CreateDbContext();
        using var transaction = context.Database.BeginTransaction();

        var snapshot = Phase3Tests.CreateOrderFixture(context);
        var manager = CreateManager(context);

        var options = manager.GetPreferenceChoicesForOrderAsync(snapshot.OrderId).GetAwaiter().GetResult();
        var persistedOptions = new ShippingOptionMapper(context).FindByOrderIdAsync(snapshot.OrderId).GetAwaiter().GetResult();

        TestAssertions.AssertEqual(3, options.Count);
        TestAssertions.AssertSequence(new[] { PreferenceType.FAST, PreferenceType.CHEAP, PreferenceType.GREEN }, options.Select(option => option.PreferenceType));
        TestAssertions.AssertEqual(0, persistedOptions.Count);

        transaction.Rollback();
    }

    private static void Feature1AppliesSelectionEndToEnd()
    {
        using var context = Phase3Tests.CreateDbContext();
        using var transaction = context.Database.BeginTransaction();

        var snapshot = Phase3Tests.CreateOrderFixture(context, includeAdditionalOrderItem: true);
        var manager = CreateManager(context);

        var result = manager.ConfirmPreferenceSelectionAsync(
            new SelectShippingPreferenceRequest(snapshot.OrderId, PreferenceType.GREEN)).GetAwaiter().GetResult();

        context.ChangeTracker.Clear();
        var persistedOptions = new ShippingOptionMapper(context).FindByOrderIdAsync(snapshot.OrderId).GetAwaiter().GetResult();

        var checkout = context.Checkouts
            .First(entity => EF.Property<int>(entity, "Checkoutid") == snapshot.CheckoutId);
        var persistedOption = persistedOptions.Single();
        var routeId = persistedOption.GetSummary().RouteId
            ?? throw new InvalidOperationException("Expected persisted option to reference a route.");
        var route = context.DeliveryRoutes
            .Include(entity => entity.RouteLegs)
            .First(entity => EF.Property<int>(entity, "RouteId") == routeId);
        var routeLegs = route.GetOrderedRouteLegs();

        TestAssertions.AssertEqual(1, persistedOptions.Count);
        TestAssertions.AssertEqual(result.OptionId, checkout.GetSelectionState().SelectedOptionId);
        TestAssertions.AssertEqual(PreferenceType.GREEN, result.PreferenceType);
        TestAssertions.AssertTrue(result.OptionId > 0, "Expected a persisted shipping option id.");
        TestAssertions.AssertEqual("TRAIN", result.TransportModeLabel);
        TestAssertions.AssertEqual(3, routeLegs.Count);
        TestAssertions.AssertTrue(routeLegs[0].GetIsFirstMile() == true, "Expected first leg to be marked as first-mile.");
        TestAssertions.AssertEqual(TransportMode.TRUCK, routeLegs[0].GetTransportMode() ?? throw new InvalidOperationException("Expected first leg transport mode."));
        TestAssertions.AssertTrue(routeLegs[1].GetIsMainTransport() == true, "Expected second leg to be marked as main transport.");
        TestAssertions.AssertEqual(TransportMode.TRAIN, routeLegs[1].GetTransportMode() ?? throw new InvalidOperationException("Expected main leg transport mode."));
        TestAssertions.AssertTrue(routeLegs[2].GetIsLastMile() == true, "Expected final leg to be marked as last-mile.");
        TestAssertions.AssertEqual(TransportMode.TRUCK, routeLegs[2].GetTransportMode() ?? throw new InvalidOperationException("Expected last leg transport mode."));

        transaction.Rollback();
    }

    private static void Feature1OrderContextResolvesRealInputs()
    {
        using var context = Phase3Tests.CreateDbContext();
        using var transaction = context.Database.BeginTransaction();

        var snapshot = Phase3Tests.CreateOrderFixture(context, includeAdditionalOrderItem: true);
        Phase3Tests.CreateOrderItemFixture(context, snapshot.OrderId, snapshot.ProductId, quantity: 1);

        var service = new ShippingOrderContextService(
            context,
            new StubInventoryService(),
            new TransportationHubMapper(context));

        var shippingContext = service.GetShippingContextAsync(snapshot.OrderId).GetAwaiter().GetResult()
            ?? throw new InvalidOperationException("Expected a shipping context result.");
        var resolvedWarehouse = new TransportationHubMapper(context).FindById(shippingContext.HubId);
        var firstProduct = shippingContext.Items.Single(item => item.ProductId == snapshot.ProductIds[0]);
        var secondProduct = shippingContext.Items.Single(item => item.ProductId == snapshot.ProductIds[1]);

        TestAssertions.AssertEqual(snapshot.OrderId, shippingContext.OrderId);
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

    private static void Feature1OrderContextRejectsOrdersWithoutItems()
    {
        using (var context = Phase3Tests.CreateDbContext())
        using (var transaction = context.Database.BeginTransaction())
        {
            var snapshot = Phase3Tests.CreateOrderFixture(context);
            context.Orderitems
                .Where(entity => EF.Property<int>(entity, "Orderid") == snapshot.OrderId)
                .ExecuteDelete();

            var service = new ShippingOrderContextService(
                context,
                new StubInventoryService(),
                new TransportationHubMapper(context));

            var missingItemsException = TestAssertions.AssertThrows<InvalidOperationException>(
                () => service.GetShippingContextAsync(snapshot.OrderId).GetAwaiter().GetResult(),
                "Expected order-context lookup to reject orders without items.");
            TestAssertions.AssertTrue(
                missingItemsException.Message.Contains("does not contain any order items", StringComparison.Ordinal),
                "Expected missing-items error message.");

            transaction.Rollback();
        }
    }

    private static void Feature1SelectionRollsBackOnFailure()
    {
        using var context = Phase3Tests.CreateDbContext();

        var snapshot = Phase3Tests.CreateOrderFixture(context, includeAdditionalOrderItem: true);
        var routeIdsBefore = context.DeliveryRoutes
            .Select(entity => EF.Property<int>(entity, "RouteId"))
            .ToArray();

        try
        {
            var manager = CreateManager(context, new ThrowingCheckoutSelectionMapper(context));

            TestAssertions.AssertThrows<InvalidOperationException>(
                () => manager.ConfirmPreferenceSelectionAsync(
                    new SelectShippingPreferenceRequest(snapshot.OrderId, PreferenceType.GREEN)).GetAwaiter().GetResult(),
                "Expected the confirm flow to surface the simulated checkout failure.");

            context.ChangeTracker.Clear();

            var persistedOptions = new ShippingOptionMapper(context).FindByOrderIdAsync(snapshot.OrderId).GetAwaiter().GetResult();
            var routeIdsAfter = context.DeliveryRoutes
                .Select(entity => EF.Property<int>(entity, "RouteId"))
                .ToArray();

            TestAssertions.AssertEqual(0, persistedOptions.Count);
            TestAssertions.AssertSequence(routeIdsBefore.OrderBy(id => id), routeIdsAfter.OrderBy(id => id));
        }
        finally
        {
            CleanupOrderFixture(context, snapshot, routeIdsBefore);
        }
    }

    private static ShippingOptionManager CreateManager(AppDbContext context, IShippingOptionMapper? mapper = null)
    {
        var hubMapper = new TransportationHubMapper(context);
        return new ShippingOptionManager(
            mapper ?? new ShippingOptionMapper(context),
            new ShippingOrderContextService(context, new StubInventoryService(), hubMapper),
            new RouteManager(context, hubMapper, new RouteLegBuilder(new StubRouteDistanceCalculator())),
            new ProRental.Domain.Module3.P2_1.Controls.TransportCarbonManager(new PricingRuleGateway(context), new StubHubCarbonService()),
            context);
    }

    private static void CleanupOrderFixture(AppDbContext context, Phase3Tests.OrderFixture snapshot, IReadOnlyCollection<int> routeIdsBefore)
    {
        context.ChangeTracker.Clear();

        var newRouteIds = context.DeliveryRoutes
            .Select(entity => EF.Property<int>(entity, "RouteId"))
            .Where(routeId => !routeIdsBefore.Contains(routeId))
            .ToArray();

        context.ShippingOptions
            .Where(entity => EF.Property<int?>(entity, "OrderId") == snapshot.OrderId)
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

        context.Orderitems
            .Where(entity => EF.Property<int>(entity, "Orderid") == snapshot.OrderId)
            .ExecuteDelete();
        context.Orders
            .Where(entity => EF.Property<int>(entity, "Orderid") == snapshot.OrderId)
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
        public List<ProductDropdownItem> GetAllProductDropdownItems() => [];
        public ProductStorageInfo? GetProductStorageInfo(int productId) => null;
        public List<ProductStorageInfo> GetAllProductStorageInfo() => [];
    }

    private sealed class StubRouteDistanceCalculator : IRouteDistanceCalculator
    {
        public double CalculateDistanceKm(string startPoint, string endPoint)
        {
            if (string.Equals(startPoint, endPoint, StringComparison.Ordinal))
            {
                return 4d;
            }

            if (startPoint.StartsWith("Phase 3 Warehouse", StringComparison.Ordinal))
            {
                return 4d;
            }

            if (string.Equals(endPoint, "Phase 3 Address", StringComparison.Ordinal))
            {
                return 16d;
            }

            return 8d;
        }
    }

    private sealed class ThrowingCheckoutSelectionMapper : IShippingOptionMapper
    {
        private readonly ShippingOptionMapper _inner;

        public ThrowingCheckoutSelectionMapper(AppDbContext context)
        {
            _inner = new ShippingOptionMapper(context);
        }

        public Task<Order?> FindOrderWithCheckoutAsync(int orderId, CancellationToken cancellationToken = default) =>
            _inner.FindOrderWithCheckoutAsync(orderId, cancellationToken);

        public Task<IReadOnlyList<ShippingOption>> FindByOrderIdAsync(int orderId, CancellationToken cancellationToken = default) =>
            _inner.FindByOrderIdAsync(orderId, cancellationToken);

        public Task<ShippingOption?> FindByIdAsync(int optionId, CancellationToken cancellationToken = default) =>
            _inner.FindByIdAsync(optionId, cancellationToken);

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
