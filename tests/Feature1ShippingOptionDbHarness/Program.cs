using Npgsql;

// Lightweight database helper for the Feature 1 Playwright flow.
// It resets seeded state and probes the persisted checkout selection without
// pulling the browser test into app-internal test hooks.
const string fallbackConnectionString = "Host=localhost;Port=5432;Database=pro_rental;Username=postgres;Password=password";

if (args.Length != 2 || !int.TryParse(args[1], out var checkoutId) || checkoutId <= 0)
{
    PrintUsage();
    return 1;
}

var commandName = args[0].Trim().ToLowerInvariant();
var connectionString = Environment.GetEnvironmentVariable("PRORENTAL_CONNECTION_STRING");
if (string.IsNullOrWhiteSpace(connectionString))
{
    connectionString = fallbackConnectionString;
}

await using var connection = new NpgsqlConnection(connectionString);
await connection.OpenAsync();

return commandName switch
{
    "get-selected-option" => await GetSelectedOptionAsync(connection, checkoutId),
    "get-option-count" => await GetOptionCountAsync(connection, checkoutId),
    "reset-checkout" => await ResetCheckoutAsync(connection, checkoutId),
    _ => UnknownCommand(commandName)
};

static int UnknownCommand(string commandName)
{
    Console.Error.WriteLine($"Unknown command '{commandName}'.");
    PrintUsage();
    return 1;
}

static async Task<int> GetSelectedOptionAsync(NpgsqlConnection connection, int checkoutId)
{
    // The browser test verifies that the selected shipping option is persisted to checkout.option_id.
    await using var command = connection.CreateCommand();
    command.CommandText =
        """
        select option_id
        from checkout
        where checkoutid = @checkoutId
        """;
    command.Parameters.AddWithValue("checkoutId", checkoutId);

    var optionId = await command.ExecuteScalarAsync();
    Console.WriteLine(optionId is null or DBNull ? "NULL" : optionId);
    return 0;
}

static async Task<int> GetOptionCountAsync(NpgsqlConnection connection, int checkoutId)
{
    await using var command = connection.CreateCommand();
    command.CommandText =
        """
        select count(*)
        from shipping_option
        where checkout_id = @checkoutId
        """;
    command.Parameters.AddWithValue("checkoutId", checkoutId);

    var count = await command.ExecuteScalarAsync();
    Console.WriteLine(count is null or DBNull ? "0" : count);
    return 0;
}

static async Task<int> ResetCheckoutAsync(NpgsqlConnection connection, int checkoutId)
{
    // Reset both the selected checkout option and any generated Feature 1 shipping rows
    // so each browser run starts from the same seeded checkout state.
    await using var transaction = await connection.BeginTransactionAsync();

    var resetCheckoutId = await ResetSelectedOptionAsync(connection, transaction, checkoutId);
    if (resetCheckoutId is null)
    {
        throw new InvalidOperationException($"Checkout '{checkoutId}' was not found.");
    }

    var routeIds = await FindRouteIdsAsync(connection, transaction, checkoutId);
    await DeleteShippingOptionsAsync(connection, transaction, checkoutId);

    if (routeIds.Count > 0)
    {
        await DeleteRouteLegsAsync(connection, transaction, routeIds);
        await DeleteRoutesAsync(connection, transaction, routeIds);
    }

    await transaction.CommitAsync();
    Console.WriteLine("RESET");
    return 0;
}

static async Task<int?> ResetSelectedOptionAsync(
    NpgsqlConnection connection,
    NpgsqlTransaction transaction,
    int checkoutId)
{
    await using var command = connection.CreateCommand();
    command.Transaction = transaction;
    command.CommandText =
        """
        update checkout
        set option_id = null
        where checkoutid = @checkoutId
        returning checkoutid
        """;
    command.Parameters.AddWithValue("checkoutId", checkoutId);

    var selectedCheckoutId = await command.ExecuteScalarAsync();
    return selectedCheckoutId is null or DBNull ? null : Convert.ToInt32(selectedCheckoutId);
}

static async Task<List<int>> FindRouteIdsAsync(
    NpgsqlConnection connection,
    NpgsqlTransaction transaction,
    int checkoutId)
{
    await using var command = connection.CreateCommand();
    command.Transaction = transaction;
    command.CommandText =
        """
        select distinct route_id
        from shipping_option
        where checkout_id = @checkoutId
          and route_id is not null
        """;
    command.Parameters.AddWithValue("checkoutId", checkoutId);

    var routeIds = new List<int>();

    await using var reader = await command.ExecuteReaderAsync();
    while (await reader.ReadAsync())
    {
        routeIds.Add(reader.GetInt32(0));
    }

    return routeIds;
}

static async Task DeleteShippingOptionsAsync(
    NpgsqlConnection connection,
    NpgsqlTransaction transaction,
    int checkoutId)
{
    await using var command = connection.CreateCommand();
    command.Transaction = transaction;
    command.CommandText =
        """
        delete from shipping_option
        where checkout_id = @checkoutId
        """;
    command.Parameters.AddWithValue("checkoutId", checkoutId);

    await command.ExecuteNonQueryAsync();
}

static async Task DeleteRoutesAsync(
    NpgsqlConnection connection,
    NpgsqlTransaction transaction,
    IReadOnlyList<int> routeIds)
{
    await using var command = connection.CreateCommand();
    command.Transaction = transaction;
    command.CommandText =
        """
        delete from delivery_route
        where route_id = any(@routeIds)
        """;
    command.Parameters.AddWithValue("routeIds", routeIds.ToArray());

    await command.ExecuteNonQueryAsync();
}

static async Task DeleteRouteLegsAsync(
    NpgsqlConnection connection,
    NpgsqlTransaction transaction,
    IReadOnlyList<int> routeIds)
{
    await using var command = connection.CreateCommand();
    command.Transaction = transaction;
    command.CommandText =
        """
        delete from route_leg
        where route_id = any(@routeIds)
        """;
    command.Parameters.AddWithValue("routeIds", routeIds.ToArray());

    await command.ExecuteNonQueryAsync();
}

static void PrintUsage()
{
    Console.Error.WriteLine("Usage: dotnet run --project tests/Feature1ShippingOptionDbHarness -- <get-selected-option|get-option-count|reset-checkout> <checkoutId>");
}
