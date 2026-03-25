using ProRental.Data.UnitOfWork;
using ProRental.Data.Module3.P2_1.Gateways;
using ProRental.Data.Module3.P2_1.Interfaces;
using ProRental.Data.Module3.P2_1.Mappers;
using ProRental.Domain.Module3.P2_1.Controls;
using ProRental.Domain.Module3.P2_1.Factories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using ProRental.Configuration.Module3.P2_1;
using ProRental.Domain.Enums;
using ProRental.Domain.Entities;
using ProRental.Testing;
using ProRental.Interfaces.Module3.P2_1;

// uncomment when ready to code
// using ProRental.Data;
// using ProRental.Domain.Controls;
// using ProRental.Domain.Entities;
// using ProRental.Interfaces.Module3.P2_1;
// using ProRental.Controllers;

//p2-1 feat 1 test

var builder = WebApplication.CreateBuilder(args);

if (args.Length > 0 && string.Equals(args[0], "--phase-tests", StringComparison.OrdinalIgnoreCase))
{
    Environment.ExitCode = await PhaseTestRunner.RunAsync(args.Skip(1).ToArray());
    return;
}
//end p2-1 feat 1 test

// Add services to the container.
builder.Services.AddControllersWithViews();

// builder.Services.AddDbContext<AppDbContext>(options =>
//     options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

var connectionString = builder.Configuration.GetConnectionString("Default");

// 2. Create the builder and map your strict PostgreSQL Enum
var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.MapEnum<AccessEventType>("access_event_type", new Npgsql.NameTranslation.NpgsqlNullNameTranslator());
dataSourceBuilder.MapEnum<AlertStatus>("alert_status", new Npgsql.NameTranslation.NpgsqlNullNameTranslator());
dataSourceBuilder.MapEnum<AnalyticsType>("analytics_type_enum", new Npgsql.NameTranslation.NpgsqlNullNameTranslator());
dataSourceBuilder.MapEnum<BatchStatus>("batch_status", new Npgsql.NameTranslation.NpgsqlNullNameTranslator());
dataSourceBuilder.MapEnum<CarbonStageType>("carbon_stage_type", new Npgsql.NameTranslation.NpgsqlNullNameTranslator());
dataSourceBuilder.MapEnum<CartStatus>("cart_status_enum", new Npgsql.NameTranslation.NpgsqlNullNameTranslator());
dataSourceBuilder.MapEnum<CheckoutStatus>("checkout_status_enum", new Npgsql.NameTranslation.NpgsqlNullNameTranslator());
dataSourceBuilder.MapEnum<ClearanceBatchStatus>("clearance_batch_status", new Npgsql.NameTranslation.NpgsqlNullNameTranslator());
dataSourceBuilder.MapEnum<ClearanceStatus>("clearance_status", new Npgsql.NameTranslation.NpgsqlNullNameTranslator());
dataSourceBuilder.MapEnum<DeliveryDuration>("delivery_duration_enum", new Npgsql.NameTranslation.NpgsqlNullNameTranslator());
dataSourceBuilder.MapEnum<DeliveryType>("delivery_type_enum", new Npgsql.NameTranslation.NpgsqlNullNameTranslator());
dataSourceBuilder.MapEnum<FileFormat>("file_format_enum", new Npgsql.NameTranslation.NpgsqlNullNameTranslator());
dataSourceBuilder.MapEnum<HubType>("hub_type", new Npgsql.NameTranslation.NpgsqlNullNameTranslator());
dataSourceBuilder.MapEnum<InventoryStatus>("inventory_status", new Npgsql.NameTranslation.NpgsqlNullNameTranslator());
dataSourceBuilder.MapEnum<LoanStatus>("loan_status", new Npgsql.NameTranslation.NpgsqlNullNameTranslator());
dataSourceBuilder.MapEnum<LogType>("log_type_enum", new Npgsql.NameTranslation.NpgsqlNullNameTranslator());
dataSourceBuilder.MapEnum<NotificationFrequency>("notification_frequency_enum", new Npgsql.NameTranslation.NpgsqlNullNameTranslator());
dataSourceBuilder.MapEnum<NotificationGranularity>("notification_granularity_enum", new Npgsql.NameTranslation.NpgsqlNullNameTranslator());
dataSourceBuilder.MapEnum<NotificationType>("notification_type_enum", new Npgsql.NameTranslation.NpgsqlNullNameTranslator());
dataSourceBuilder.MapEnum<OrderHistoryStatus>("order_history_status_enum", new Npgsql.NameTranslation.NpgsqlNullNameTranslator());
dataSourceBuilder.MapEnum<OrderStatus>("order_status_enum", new Npgsql.NameTranslation.NpgsqlNullNameTranslator());
dataSourceBuilder.MapEnum<PaymentMethod>("payment_method_enum", new Npgsql.NameTranslation.NpgsqlNullNameTranslator());
dataSourceBuilder.MapEnum<PaymentPurpose>("payment_purpose_enum", new Npgsql.NameTranslation.NpgsqlNullNameTranslator());
dataSourceBuilder.MapEnum<POStatus>("po_status_enum", new Npgsql.NameTranslation.NpgsqlNullNameTranslator());
dataSourceBuilder.MapEnum<PreferenceType>("preference_type", new Npgsql.NameTranslation.NpgsqlNullNameTranslator());
dataSourceBuilder.MapEnum<ProductStatus>("product_status", new Npgsql.NameTranslation.NpgsqlNullNameTranslator());
dataSourceBuilder.MapEnum<PurchaseOrderStatus>("purchase_order_status_enum", new Npgsql.NameTranslation.NpgsqlNullNameTranslator());
dataSourceBuilder.MapEnum<RatingBand>("rating_band_enum", new Npgsql.NameTranslation.NpgsqlNullNameTranslator());
dataSourceBuilder.MapEnum<RentalStatus>("rental_status_enum", new Npgsql.NameTranslation.NpgsqlNullNameTranslator());
dataSourceBuilder.MapEnum<ReplenishmentReason>("reason_code_enum", new Npgsql.NameTranslation.NpgsqlNullNameTranslator());
dataSourceBuilder.MapEnum<ReplenishmentStatus>("replenishment_status_enum", new Npgsql.NameTranslation.NpgsqlNullNameTranslator());
dataSourceBuilder.MapEnum<ReturnItemStatus>("return_item_status", new Npgsql.NameTranslation.NpgsqlNullNameTranslator());
dataSourceBuilder.MapEnum<ReturnRequestStatus>("return_request_status", new Npgsql.NameTranslation.NpgsqlNullNameTranslator());
dataSourceBuilder.MapEnum<ReturnStatus>("return_status_enum", new Npgsql.NameTranslation.NpgsqlNullNameTranslator());
dataSourceBuilder.MapEnum<ShipmentStatus>("shipment_status_enum", new Npgsql.NameTranslation.NpgsqlNullNameTranslator());
dataSourceBuilder.MapEnum<StageType>("stagetype", new Npgsql.NameTranslation.NpgsqlNullNameTranslator());
dataSourceBuilder.MapEnum<SupplierCategory>("supplier_category_enum", new Npgsql.NameTranslation.NpgsqlNullNameTranslator());
dataSourceBuilder.MapEnum<TransactionPurpose>("transaction_purpose_enum", new Npgsql.NameTranslation.NpgsqlNullNameTranslator());
dataSourceBuilder.MapEnum<TransactionStatus>("transaction_status_enum", new Npgsql.NameTranslation.NpgsqlNullNameTranslator());
dataSourceBuilder.MapEnum<TransactionType>("transaction_type_enum", new Npgsql.NameTranslation.NpgsqlNullNameTranslator());
dataSourceBuilder.MapEnum<TransportMode>("transport_mode", new Npgsql.NameTranslation.NpgsqlNullNameTranslator());
dataSourceBuilder.MapEnum<UserRole>("user_role_enum", new Npgsql.NameTranslation.NpgsqlNullNameTranslator());
dataSourceBuilder.MapEnum<VettingDecision>("vetting_decision_enum", new Npgsql.NameTranslation.NpgsqlNullNameTranslator());
dataSourceBuilder.MapEnum<VettingResult>("vetting_result_enum", new Npgsql.NameTranslation.NpgsqlNullNameTranslator());
dataSourceBuilder.MapEnum<VisualType>("visual_type_enum", new Npgsql.NameTranslation.NpgsqlNullNameTranslator());

// 3. Build the data source
var dataSource = dataSourceBuilder.Build();

// 4. Register the DbContext using the data source instead of a raw string
// builder.Services.AddDbContext<AppDbContext>(options =>
//     options.UseNpgsql(dataSource));
var nullTranslator = new Npgsql.NameTranslation.NpgsqlNullNameTranslator();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(dataSource, o =>
    {
        o.MapEnum<AccessEventType>("access_event_type", nameTranslator: nullTranslator);
        o.MapEnum<AlertStatus>("alert_status", nameTranslator: nullTranslator);
        o.MapEnum<AnalyticsType>("analytics_type_enum", nameTranslator: nullTranslator);
        o.MapEnum<BatchStatus>("batch_status", nameTranslator: nullTranslator);
        o.MapEnum<CarbonStageType>("carbon_stage_type", nameTranslator: nullTranslator);
        o.MapEnum<CartStatus>("cart_status_enum", nameTranslator: nullTranslator);
        o.MapEnum<CheckoutStatus>("checkout_status_enum", nameTranslator: nullTranslator);
        o.MapEnum<ClearanceBatchStatus>("clearance_batch_status", nameTranslator: nullTranslator);
        o.MapEnum<ClearanceStatus>("clearance_status", nameTranslator: nullTranslator);
        o.MapEnum<DeliveryDuration>("delivery_duration_enum", nameTranslator: nullTranslator);
        o.MapEnum<DeliveryType>("delivery_type_enum", nameTranslator: nullTranslator);
        o.MapEnum<FileFormat>("file_format_enum", nameTranslator: nullTranslator);
        o.MapEnum<HubType>("hub_type", nameTranslator: nullTranslator);
        o.MapEnum<InventoryStatus>("inventory_status", nameTranslator: nullTranslator);
        o.MapEnum<LoanStatus>("loan_status", nameTranslator: nullTranslator);
        o.MapEnum<LogType>("log_type_enum", nameTranslator: nullTranslator);
        o.MapEnum<NotificationFrequency>("notification_frequency_enum", nameTranslator: nullTranslator);
        o.MapEnum<NotificationGranularity>("notification_granularity_enum", nameTranslator: nullTranslator);
        o.MapEnum<NotificationType>("notification_type_enum", nameTranslator: nullTranslator);
        o.MapEnum<OrderHistoryStatus>("order_history_status_enum", nameTranslator: nullTranslator);
        o.MapEnum<OrderStatus>("order_status_enum", nameTranslator: nullTranslator);
        o.MapEnum<PaymentMethod>("payment_method_enum", nameTranslator: nullTranslator);
        o.MapEnum<PaymentPurpose>("payment_purpose_enum", nameTranslator: nullTranslator);
        o.MapEnum<POStatus>("po_status_enum", nameTranslator: nullTranslator);
        o.MapEnum<PreferenceType>("preference_type", nameTranslator: nullTranslator);
        o.MapEnum<ProductStatus>("product_status", nameTranslator: nullTranslator);
        o.MapEnum<PurchaseOrderStatus>("purchase_order_status_enum", nameTranslator: nullTranslator);
        o.MapEnum<RatingBand>("rating_band_enum", nameTranslator: nullTranslator);
        o.MapEnum<RentalStatus>("rental_status_enum", nameTranslator: nullTranslator);
        o.MapEnum<ReplenishmentReason>("reason_code_enum", nameTranslator: nullTranslator);
        o.MapEnum<ReplenishmentStatus>("replenishment_status_enum", nameTranslator: nullTranslator);
        o.MapEnum<ReturnItemStatus>("return_item_status", nameTranslator: nullTranslator);
        o.MapEnum<ReturnRequestStatus>("return_request_status", nameTranslator: nullTranslator);
        o.MapEnum<ReturnStatus>("return_status_enum", nameTranslator: nullTranslator);
        o.MapEnum<ShipmentStatus>("shipment_status_enum", nameTranslator: nullTranslator);
        o.MapEnum<StageType>("stagetype", nameTranslator: nullTranslator);
        o.MapEnum<SupplierCategory>("supplier_category_enum", nameTranslator: nullTranslator);
        o.MapEnum<TransactionPurpose>("transaction_purpose_enum", nameTranslator: nullTranslator);
        o.MapEnum<TransactionStatus>("transaction_status_enum", nameTranslator: nullTranslator);
        o.MapEnum<TransactionType>("transaction_type_enum", nameTranslator: nullTranslator);
        o.MapEnum<TransportMode>("transport_mode", nameTranslator: nullTranslator);
        o.MapEnum<UserRole>("user_role_enum", nameTranslator: nullTranslator);
        o.MapEnum<VettingDecision>("vetting_decision_enum", nameTranslator: nullTranslator);
        o.MapEnum<VettingResult>("vetting_result_enum", nameTranslator: nullTranslator);
        o.MapEnum<VisualType>("visual_type_enum", nameTranslator: nullTranslator);
    })
    .EnableSensitiveDataLogging()
    .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information));

//Services builder(add your mappers/gateways, controllers, control and interface classes here)
//Team P2-1
// Data source
builder.Services.AddScoped<ProRental.Data.Interfaces.ITransportationHubMapper, ProRental.Data.Gateways.TransportationHubMapper>();

// Domain
builder.Services.AddScoped<ProRental.Domain.Control.TransportationHubFactory>();
builder.Services.AddScoped<ProRental.Interfaces.Module3.P2_1.IHubCarbonService, ProRental.Domain.Control.TransportationHubManager>();
builder.Services.AddScoped<ProRental.Interfaces.Module3.P2_1.IHubInfoService, ProRental.Domain.Control.TransportationHubManager>();
builder.Services.AddScoped<ProRental.Interfaces.IInventoryService, ProRental.Domain.Control.DummyInventoryService>(); // TODO: Replace with Module 2's real implementation
builder.Services.AddFeature1Services();
//TODO: ADD THIS INTO A REGISTRATION
builder.Services.AddScoped<ITransportMapper, TransportMapper>();
builder.Services.AddScoped<TruckMapper>();
builder.Services.AddScoped<ShipMapper>();
builder.Services.AddScoped<PlaneMapper>();
builder.Services.AddScoped<TrainMapper>();
builder.Services.AddScoped<IPricingRuleGateway, PricingRuleGateway>();
builder.Services.AddScoped<ProRental.Data.Module3.P2_1.Interfaces.IReturnStageGateway, ProRental.Data.Module3.P2_1.Gateways.ReturnStageGateway>();

// Domain
builder.Services.AddScoped<ProRental.Domain.Module3.P2_1.Controls.ReturnStageCalculator>();
builder.Services.AddScoped<ProRental.Domain.Module3.P2_1.Controls.ReturnStageSurchargeService>();
builder.Services.AddScoped<ProRental.Domain.Module3.P2_1.Controls.ReturnCarbonReportService>();
builder.Services.AddScoped<IRouteDistanceCalculator, RouteDistanceCalculator>();
builder.Services.AddScoped<ITransportService, TransportationManager>();
builder.Services.AddScoped<ITransportCarbonService, TransportCarbonManager>();
builder.Services.AddScoped<TransportationFactory>();

// Presentation/Controllers
builder.Services.AddScoped<ProRental.Controllers.Module3.P2_1.ReturnStageController>();


//Team P2-2
// Data source

// Domain

// Presentation/Controllers

//Team P2-3
// Data source

// Domain

// Presentation/Controllers


//Team P2-4
// Data source

// Domain

// Presentation/Controllers


//Team P2-5
// Data source

// Domain

// Presentation/Controllers


//Team P2-6
// Data source

// Domain

// Presentation/Controllers


var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
