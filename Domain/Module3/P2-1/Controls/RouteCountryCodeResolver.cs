using ProRental.Domain.Entities;

namespace ProRental.Domain.Module3.P2_1.Controls;

internal static class RouteCountryCodeResolver
{
    private static readonly IReadOnlyDictionary<string, string> CountryCodeAliases =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["SG"] = "SG",
            ["SINGAPORE"] = "SG",
            ["JP"] = "JP",
            ["JAPAN"] = "JP",
            ["US"] = "US",
            ["USA"] = "US",
            ["UNITED STATES"] = "US",
            ["UNITED STATES OF AMERICA"] = "US"
        };

    public static string ResolveWarehouseCountryCode(TransportationHub warehouseHub)
    {
        var countryCode = NormalizeCountryCode(warehouseHub.GetCountryCode());
        if (!string.IsNullOrWhiteSpace(countryCode))
        {
            return countryCode;
        }

        return ResolveAddressCountryCode(warehouseHub.GetAddress(), "Warehouse address");
    }

    public static bool TryResolveWarehouseCountryCode(TransportationHub? warehouseHub, out string countryCode)
    {
        countryCode = string.Empty;
        if (warehouseHub is null)
        {
            return false;
        }

        countryCode = NormalizeCountryCode(warehouseHub.GetCountryCode())
            ?? NormalizeCountryCode(GetTrailingAddressSegment(warehouseHub.GetAddress()))
            ?? string.Empty;

        return !string.IsNullOrWhiteSpace(countryCode);
    }

    public static string ResolveAddressCountryCode(string address, string label)
    {
        var normalizedCountryCode = NormalizeCountryCode(GetTrailingAddressSegment(address));
        if (!string.IsNullOrWhiteSpace(normalizedCountryCode))
        {
            return normalizedCountryCode;
        }

        throw new RouteResolutionException($"{label} must end with a supported country name or country code for PLANE/SHIP route generation.");
    }

    public static bool TryResolveAddressCountryCode(string address, out string countryCode)
    {
        countryCode = NormalizeCountryCode(GetTrailingAddressSegment(address)) ?? string.Empty;
        return !string.IsNullOrWhiteSpace(countryCode);
    }

    public static string? NormalizeCountryCode(string? rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return null;
        }

        var trimmed = rawValue.Trim();
        if (CountryCodeAliases.TryGetValue(trimmed, out var exactMatch))
        {
            return exactMatch;
        }

        foreach (var alias in CountryCodeAliases)
        {
            if (trimmed.Contains(alias.Key, StringComparison.OrdinalIgnoreCase))
            {
                return alias.Value;
            }
        }

        return null;
    }

    private static string? GetTrailingAddressSegment(string? address)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            return null;
        }

        return address.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
    }
}
