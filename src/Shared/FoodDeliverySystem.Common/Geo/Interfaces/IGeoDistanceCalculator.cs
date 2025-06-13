using FoodDeliverySystem.Common.Geo.Models;

namespace FoodDeliverySystem.Common.Geo.Interfaces;

public interface IGeoDistanceCalculator
{
    Task<double> CalculateDistanceAsync(double lat1, double lon1, double lat2, double lon2, GeoUnit unit = GeoUnit.Kilometers);
    Task<bool> IsWithinRadiusAsync(double lat1, double lon1, double lat2, double lon2, double radius, GeoUnit unit = GeoUnit.Kilometers);
    Task<List<double>> CalculateDistancesBatchAsync(List<(double lat1, double lon1, double lat2, double lon2)> points, GeoUnit unit = GeoUnit.Kilometers);
    Task<List<GeoResult>> FindWithinRadiusBatchAsync(double lat, double lon, double radius, List<(double lat, double lon)> points, GeoUnit unit = GeoUnit.Kilometers);
    Task<decimal> CalculateDeliveryFeeAsync(double lat1, double lon1, double lat2, double lon2, GeoUnit unit = GeoUnit.Kilometers);
    Task<TimeSpan> CalculateETAAsync(double lat1, double lon1, double lat2, double lon2, double averageSpeedKmH = 30, GeoUnit unit = GeoUnit.Kilometers);
}