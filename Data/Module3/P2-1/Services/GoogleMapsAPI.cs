using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using ProRental.Configuration.Module3.P2_1;
using ProRental.Domain.Module3.P2_1.Controls;
using ProRental.Interfaces.Module3.P2_1;

namespace ProRental.Data.Module3.P2_1.Services;

/// <summary>
/// Thin Google Maps Routes API boundary used by the route-distance feature when
/// local configuration provides a valid API key.
/// </summary>
public sealed class GoogleMapsAPI : IGoogleMapsAPI
{
    private const string RoutesEndpoint = "directions/v2:computeRoutes";

    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public GoogleMapsAPI(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<double> FetchRouteDistanceKmAsync(
        string origin,
        string destination,
        CancellationToken cancellationToken = default)
    {
        var apiKey = _configuration[GoogleMapsConfigurationDiagnostics.ApiKeyConfigPath];
        if (string.IsNullOrWhiteSpace(apiKey) ||
            string.IsNullOrWhiteSpace(origin) ||
            string.IsNullOrWhiteSpace(destination))
        {
            throw new RouteResolutionException("Google Maps route lookup requires a configured API key and non-empty route endpoints.");
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, RoutesEndpoint)
        {
            Content = JsonContent.Create(new
            {
                origin = new
                {
                    address = origin
                },
                destination = new
                {
                    address = destination
                },
                travelMode = "DRIVE",
                routingPreference = "TRAFFIC_UNAWARE"
            })
        };

        request.Headers.TryAddWithoutValidation("X-Goog-Api-Key", apiKey);
        request.Headers.TryAddWithoutValidation("X-Goog-FieldMask", "routes.distanceMeters");

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new RouteResolutionException(
                $"Google Maps route lookup failed for '{origin}' to '{destination}' with status {(int)response.StatusCode} ({response.ReasonPhrase}).");
        }

        var payload = await response.Content.ReadFromJsonAsync<ComputeRoutesResponse>(cancellationToken: cancellationToken);
        var distanceMeters = payload?.Routes?.FirstOrDefault()?.DistanceMeters;
        if (!distanceMeters.HasValue)
        {
            throw new RouteResolutionException($"Google Maps did not return a route distance for '{origin}' to '{destination}'.");
        }

        return Math.Round(distanceMeters.Value / 1000d, 2, MidpointRounding.AwayFromZero);
    }

    private sealed class ComputeRoutesResponse
    {
        public List<RoutePayload>? Routes { get; init; }
    }

    private sealed class RoutePayload
    {
        public double? DistanceMeters { get; init; }
    }
}
