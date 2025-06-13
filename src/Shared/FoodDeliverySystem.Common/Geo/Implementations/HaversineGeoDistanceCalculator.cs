using Microsoft.Extensions.Logging;
using FoodDeliverySystem.Common.Geo.Interfaces;
using FoodDeliverySystem.Common.Geo.Models;

namespace FoodDeliverySystem.Common.Geo.Implementations;

public class HaversineGeoDistanceCalculator : IGeoDistanceCalculator
{
    private readonly ILogger<HaversineGeoDistanceCalculator> _logger;
    private const double EarthRadiusMeters = 6371000;

    public HaversineGeoDistanceCalculator(ILogger<HaversineGeoDistanceCalculator> logger)
    {
        _logger = logger;
    }

    public async Task<double> CalculateDistanceAsync(double lat1, double lon1, double lat2, double lon2, GeoUnit unit = GeoUnit.Kilometers)
    {
        _logger.LogInformation("Calculating distance with Haversine for ({Lat1}, {Lon1}) to ({Lat2}, {Lon2})", lat1, lon1, lat2, lon2);
        try
        {
            ValidateCoordinates(lat1, lon1);
            ValidateCoordinates(lat2, lon2);

            var lat1Rad = ToRadians(lat1);
            var lon1Rad = ToRadians(lon1);
            var lat2Rad = ToRadians(lat2);
            var lon2Rad = ToRadians(lon2);

            var dLat = lat2Rad - lat1Rad;
            var dLon = lon2Rad - lon1Rad;

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var distanceInMeters = EarthRadiusMeters * c;

            return unit switch
            {
                GeoUnit.Kilometers => distanceInMeters / 1000,
                GeoUnit.Miles => distanceInMeters / 1609.34,
                _ => distanceInMeters
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating distance for ({Lat1}, {Lon1}) to ({Lat2}, {Lon2})", lat1, lon1, lat2, lon2);
            throw;
        }
    }

    public async Task<bool> IsWithinRadiusAsync(double lat1, double lon1, double lat2, double lon2, double radius, GeoUnit unit = GeoUnit.Kilometers)
    {
        var distance = await CalculateDistanceAsync(lat1, lon1, lat2, lon2, unit);
        return distance <= radius;
    }

    public async Task<List<double>> CalculateDistancesBatchAsync(List<(double lat1, double lon1, double lat2, double lon2)> points, GeoUnit unit = GeoUnit.Kilometers)
    {
        var results = new List<double>();
        foreach (var (lat1, lon1, lat2, lon2) in points)
        {
            var distance = await CalculateDistanceAsync(lat1, lon1, lat2, lon2, unit);
            results.Add(distance);
        }
        return results;
    }

    public async Task<List<GeoResult>> FindWithinRadiusBatchAsync(double lat, double lon, double radius, List<(double lat, double lon)> points, GeoUnit unit = GeoUnit.Kilometers)
    {
        var results = new List<GeoResult>();
        for (int i = 0; i < points.Count; i++)
        {
            var (pointLat, pointLon) = points[i];
            var distance = await CalculateDistanceAsync(lat, lon, pointLat, pointLon, unit);
            if (distance <= radius)
            {
                results.Add(new GeoResult { Id = $"Point_{i}", Distance = distance, Latitude = pointLat, Longitude = pointLon });
            }
        }
        return results;
    }

    public async Task<decimal> CalculateDeliveryFeeAsync(double lat1, double lon1, double lat2, double lon2, GeoUnit unit = GeoUnit.Kilometers)
    {
        var distance = await CalculateDistanceAsync(lat1, lon1, lat2, lon2, unit);
        return (decimal)distance * 0.5m;
    }

    public async Task<TimeSpan> CalculateETAAsync(double lat1, double lon1, double lat2, double lon2, double averageSpeedKmH = 30, GeoUnit unit = GeoUnit.Kilometers)
    {
        var distance = await CalculateDistanceAsync(lat1, lon1, lat2, lon2, unit);
        var hours = distance / averageSpeedKmH;
        return TimeSpan.FromHours(hours);
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;

    private void ValidateCoordinates(double lat, double lon)
    {
        if (double.IsNaN(lat) || double.IsNaN(lon) || lat < -90 || lat > 90 || lon < -180 || lon > 180)
            throw new ArgumentException("Invalid coordinates.");
    }
}