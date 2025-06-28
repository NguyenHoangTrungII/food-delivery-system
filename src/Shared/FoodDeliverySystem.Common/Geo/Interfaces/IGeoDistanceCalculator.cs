using FoodDeliverySystem.Common.Geo.Models;

namespace FoodDeliverySystem.Common.Geo.Interfaces;

public interface IGeoDistanceCalculator
{
    Task<double> CalculateDistanceAsync(double lat1, double lon1, double lat2, double lon2, GeoUnit unit = GeoUnit.Kilometers, CancellationToken cancellationToken = default);
    Task<bool> IsWithinRadiusAsync(double lat1, double lon1, double lat2, double lon2, double radius, GeoUnit unit = GeoUnit.Kilometers, CancellationToken cancellationToken = default);
    //Task<List<double>> CalculateDistancesBatchAsync(List<(double lat1, double lon1, double lat2, double lon2)> points, GeoUnit unit = GeoUnit.Kilometers, CancellationToken cancellationToken = default);
    Task<List<GeoResult>> FindWithinRadiusBatchAsync(double lat, double lon, double radius, List<(string id, double lat, double lon)> points, GeoUnit unit = GeoUnit.Kilometers, CancellationToken cancellationToken = default);

    //Task<decimal> CalculateDeliveryFeeAsync(double lat1, double lon1, double lat2, double lon2, GeoUnit unit = GeoUnit.Kilometers, CancellationToken cancellationToken = default);
    //Task<TimeSpan> CalculateETAAsync(double lat1, double lon1, double lat2, double lon2, double averageSpeedKmH = 30, GeoUnit unit = GeoUnit.Kilometers, CancellationToken cancellationToken = default);
    //Task<List<GeoResult>> CalculateDistancesToPointsAsync(
    //   double lat, double lon,
    //   List<(string id, double lat, double lon)> points,
    //   GeoUnit unit = GeoUnit.Kilometers,
    //   CancellationToken cancellationToken = default);
}
