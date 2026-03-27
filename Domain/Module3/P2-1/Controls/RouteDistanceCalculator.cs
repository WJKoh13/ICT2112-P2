using ProRental.Domain.Enums;
using ProRental.Interfaces.Module3.P2_1;

namespace ProRental.Domain.Module3.P2_1.Controls;

public sealed class RouteDistanceCalculator : IRouteDistanceCalculator
{
    private readonly IGoogleMapsAPI _googleMapsApi;

    public RouteDistanceCalculator(IGoogleMapsAPI googleMapsApi)
    {
        _googleMapsApi = googleMapsApi;
    }

    public async Task<double> CalculateLegDistanceKmAsync(
        TransportMode transportMode,
        RouteDistancePoint startPoint,
        RouteDistancePoint endPoint)
    {
        var distanceKm = transportMode switch
        {
            TransportMode.PLANE or TransportMode.SHIP => CalculateGeodesicDistanceKm(transportMode, startPoint, endPoint),
            _ => await _googleMapsApi.FetchRouteDistanceKmAsync(startPoint.Address, endPoint.Address)
        };

        if (double.IsNaN(distanceKm) || double.IsInfinity(distanceKm) || distanceKm < 0d)
        {
            throw new RouteResolutionException($"Resolved route distance for '{startPoint.Address}' to '{endPoint.Address}' was invalid.");
        }

        return distanceKm;
    }

    private static double CalculateGeodesicDistanceKm(
        TransportMode transportMode,
        RouteDistancePoint startPoint,
        RouteDistancePoint endPoint)
    {
        var (startLatitude, startLongitude) = GetValidatedCoordinates(transportMode, startPoint, "origin");
        var (endLatitude, endLongitude) = GetValidatedCoordinates(transportMode, endPoint, "destination");

        const double earthRadiusKm = 6371.0088d;
        var latitudeDelta = DegreesToRadians(endLatitude - startLatitude);
        var longitudeDelta = DegreesToRadians(endLongitude - startLongitude);
        var startLatitudeRadians = DegreesToRadians(startLatitude);
        var endLatitudeRadians = DegreesToRadians(endLatitude);

        var haversine =
            Math.Pow(Math.Sin(latitudeDelta / 2d), 2d) +
            Math.Cos(startLatitudeRadians) *
            Math.Cos(endLatitudeRadians) *
            Math.Pow(Math.Sin(longitudeDelta / 2d), 2d);

        var angularDistance = 2d * Math.Atan2(Math.Sqrt(haversine), Math.Sqrt(1d - haversine));
        return Math.Round(earthRadiusKm * angularDistance, 2, MidpointRounding.AwayFromZero);
    }

    private static (double Latitude, double Longitude) GetValidatedCoordinates(
        TransportMode transportMode,
        RouteDistancePoint point,
        string label)
    {
        if (!point.Latitude.HasValue || !point.Longitude.HasValue)
        {
            throw new RouteResolutionException(
                $"Valid hub coordinates are required for {transportMode} route distance resolution on the {label} endpoint '{point.Address}'.");
        }

        var latitude = point.Latitude.Value;
        var longitude = point.Longitude.Value;
        if (double.IsNaN(latitude) ||
            double.IsInfinity(latitude) ||
            latitude is < -90d or > 90d ||
            double.IsNaN(longitude) ||
            double.IsInfinity(longitude) ||
            longitude is < -180d or > 180d)
        {
            throw new RouteResolutionException(
                $"Valid hub coordinates are required for {transportMode} route distance resolution on the {label} endpoint '{point.Address}'.");
        }

        return (latitude, longitude);
    }

    private static double DegreesToRadians(double degrees) => degrees * (Math.PI / 180d);
}
