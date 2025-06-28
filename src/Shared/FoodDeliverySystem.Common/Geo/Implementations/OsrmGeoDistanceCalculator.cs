using FoodDeliverySystem.Common.Geo.Common;
using FoodDeliverySystem.Common.Geo.Common.Enums;
using FoodDeliverySystem.Common.Geo.Interfaces;
using FoodDeliverySystem.Common.Geo.Models;
using Microsoft.Extensions.Logging;

namespace FoodDeliverySystem.Common.Geo.Implementations.DistanceCalculators;

public class OsrmGeoDistanceCalculator : IGeoDistanceCalculator
{
    private readonly IMapApiClient _mapApiClient;
    // NEW: Fallback Haversine khi OSRM thất bại
    private readonly HaversineGeoDistanceCalculator _fallbackCalculator;
    private readonly ILogger<OsrmGeoDistanceCalculator> _logger;
    private readonly ILogger<HaversineGeoDistanceCalculator> _logger_;


    public OsrmGeoDistanceCalculator(IMapApiClient mapApiClient, ILogger<OsrmGeoDistanceCalculator> logger1, ILogger<HaversineGeoDistanceCalculator> logger2)
    {
        _mapApiClient = mapApiClient ?? throw new ArgumentNullException(nameof(mapApiClient));
        _logger = logger1 ?? throw new ArgumentNullException(nameof(logger1));
        _fallbackCalculator = new HaversineGeoDistanceCalculator(logger2);
    }

    public async Task<double> CalculateDistanceAsync(double lat1, double lon1, double lat2, double lon2, GeoUnit unit = GeoUnit.Kilometers, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        // CHANGED: Sử dụng GeoValidator
        GeoValidator.ValidateCoordinates(lat1, lon1, lat2, lon2);
        try
        {
            var (distance, _) = await _mapApiClient.GetRoadDistanceAsync(lat1, lon1, lat2, lon2, unit, cancellationToken);
            return distance;
        }
        catch (HttpRequestException ex)
        {
            // NEW: Fallback Haversine
            _logger.LogWarning(ex, "OSRM unavailable, falling back to Haversine");
            return await _fallbackCalculator.CalculateDistanceAsync(lat1, lon1, lat2, lon2, unit, cancellationToken);
        }
    }

    public async Task<bool> IsWithinRadiusAsync(double lat1, double lon1, double lat2, double lon2, double radius, GeoUnit unit = GeoUnit.Kilometers, CancellationToken cancellationToken = default)
    {
        var distance = await CalculateDistanceAsync(lat1, lon1, lat2, lon2, unit, cancellationToken);
        return distance <= radius;
    }

    // CHANGED: Tối ưu batch với Task.WhenAll
    public async Task<List<double>> CalculateDistancesBatchAsync(List<(double lat1, double lon1, double lat2, double lon2)> points, GeoUnit unit = GeoUnit.Kilometers, CancellationToken cancellationToken = default)
    {
        var tasks = points.Select(p => CalculateDistanceAsync(p.lat1, p.lon1, p.lat2, p.lon2, unit, cancellationToken));
        return (await Task.WhenAll(tasks)).ToList();
    }

    // CHANGED: Tối ưu batch với Task.WhenAll
    public async Task<List<GeoResult>> FindWithinRadiusBatchAsync(
        double lat, double lon, double radius, List<(string id, double lat, double lon)> points, GeoUnit unit = GeoUnit.Kilometers,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        GeoValidator.ValidateCoordinates(lat, lon);
        var tasks = points.Select(async p =>
        {
            var distance = await CalculateDistanceAsync(lat, lon, p.lat, p.lon, unit, cancellationToken);
            if (distance <= radius)
            {
                return new GeoResult { Id = p.id, Distance = distance, Latitude = p.lat, Longitude = p.lon };
            }
            return null;
        });
        var results = await Task.WhenAll(tasks);
        return results.Where(r => r != null).ToList();
    }

    // CHANGED: Sửa lỗi, trả về điểm ngoài bán kính
    public async Task<List<GeoResult>> FindWithoutinRadiusBatchAsync(
        double lat, double lon, double radius, List<(string id, double lat, double lon)> points, GeoUnit unit = GeoUnit.Kilometers,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        GeoValidator.ValidateCoordinates(lat, lon);
        var tasks = points.Select(async p =>
        {
            var distance = await CalculateDistanceAsync(lat, lon, p.lat, p.lon, unit, cancellationToken);
            if (distance > radius) // CHANGED: Lọc điểm ngoài bán kính
            {
                return new GeoResult { Id = p.id, Distance = distance, Latitude = p.lat, Longitude = p.lon };
            }
            return null;
        });
        var results = await Task.WhenAll(tasks);
        return results.Where(r => r != null).ToList();
    }

    public async Task<List<GeoResult>> CalculateDistancesToPointsAsync(
        double lat, double lon,
        List<(string id, double lat, double lon)> points,
        GeoUnit unit = GeoUnit.Kilometers,
        CancellationToken cancellationToken = default)
    {
        var tasks = points.Select(async p =>
        {
            var distance = await CalculateDistanceAsync(lat, lon, p.lat, p.lon, unit, cancellationToken);
            return new GeoResult
            {
                Id = p.id,
                Distance = distance,
                Latitude = p.lat,
                Longitude = p.lon
            };
        });
        return (await Task.WhenAll(tasks)).ToList();
    }

    // CHANGED: Chuyển sang IDeliveryFeeCalculator
    //public async Task<decimal> CalculateDeliveryFeeAsync(double lat1, double lon1, double lat2, double lon2, GeoUnit unit = GeoUnit.Kilometers, CancellationToken cancellationToken = default)
    //{
    //    throw new NotSupportedException("Use IDeliveryFeeCalculator instead.");
    //}

    //// CHANGED: Chuyển sang IETACalculator
    //public async Task<TimeSpan> CalculateETAAsync(double lat1, double lon1, double lat2, double lon2, double averageSpeedKmH = 30, GeoUnit unit = GeoUnit.Kilometers, CancellationToken cancellationToken = default)
    //{
    //    throw new NotSupportedException("Use IETACalculator instead.");
    //}
}

//using FoodDeliverySystem.Common.Geo.Interfaces;
//using FoodDeliverySystem.Common.Geo.Models;
//using System.Threading;

//namespace FoodDeliverySystem.Common.Geo.Implementations;

//public class OsrmGeoDistanceCalculator : IGeoDistanceCalculator
//{
//    private readonly IMapApiClient _mapApiClient;

//    public OsrmGeoDistanceCalculator(IMapApiClient mapApiClient)
//    {
//        _mapApiClient = mapApiClient ?? throw new ArgumentNullException(nameof(mapApiClient));
//    }

//    public async Task<double> CalculateDistanceAsync(double lat1, double lon1, double lat2, double lon2, GeoUnit unit = GeoUnit.Kilometers, CancellationToken cancellationToken = default)
//    {
//        cancellationToken.ThrowIfCancellationRequested();
//        var (distance, _) = await _mapApiClient.GetRoadDistanceAsync(lat1, lon1, lat2, lon2, unit, cancellationToken);
//        return distance;
//    }

//    public async Task<bool> IsWithinRadiusAsync(double lat1, double lon1, double lat2, double lon2, double radius, GeoUnit unit = GeoUnit.Kilometers, CancellationToken cancellationToken = default)
//    {
//        cancellationToken.ThrowIfCancellationRequested();
//        var distance = await CalculateDistanceAsync(lat1, lon1, lat2, lon2, unit, cancellationToken);
//        return distance <= radius;
//    }

//    public async Task<List<double>> CalculateDistancesBatchAsync(List<(double lat1, double lon1, double lat2, double lon2)> points, GeoUnit unit = GeoUnit.Kilometers, CancellationToken cancellationToken = default)
//    {
//        var results = new List<double>();
//        foreach (var (lat1, lon1, lat2, lon2) in points)
//        {
//            cancellationToken.ThrowIfCancellationRequested();
//            var distance = await CalculateDistanceAsync(lat1, lon1, lat2, lon2, unit, cancellationToken);
//            results.Add(distance);
//        }
//        return results;
//    }

//    //public async Task<List<GeoResult>> FindWithinRadiusBatchAsync(double lat, double lon, double radius, List<(string id, double lat, double lon)> points, GeoUnit unit = GeoUnit.Kilometers, CancellationToken cancellationToken = default)
//    //{
//    //    cancellationToken.ThrowIfCancellationRequested();
//    //    var results = new List<GeoResult>();
//    //    foreach (var (pointLat, pointLon) in points)
//    //    {
//    //        cancellationToken.ThrowIfCancellationRequested();
//    //        var distance = await CalculateDistanceAsync(lat, lon, pointLat, pointLon, unit, cancellationToken);
//    //        if (distance <= radius)
//    //        {
//    //            results.Add(new GeoResult { Id = Guid.NewGuid().ToString(), Distance = distance, Latitude = pointLat, Longitude = pointLon });
//    //        }
//    //    }
//    //    return results;
//    //}

//    public async Task<List<GeoResult>> FindWithinRadiusBatchAsync(
//       double lat,
//       double lon,
//       double radius,
//       List<(string id, double lat, double lon)> points,
//       GeoUnit unit = GeoUnit.Kilometers,
//       CancellationToken cancellationToken = default)
//    {
//        cancellationToken.ThrowIfCancellationRequested();
//        var results = new List<GeoResult>();
//        foreach (var (id, pointLat, pointLon) in points)
//        {
//            cancellationToken.ThrowIfCancellationRequested();
//            var distance = await CalculateDistanceAsync(lat, lon, pointLat, pointLon, unit, cancellationToken);
//            if (distance <= radius) // Bỏ comment để chỉ trả về các điểm trong bán kính
//            {
//                results.Add(new GeoResult
//                {
//                    Id = id, // Sử dụng id của nhà hàng
//                    Distance = distance,
//                    Latitude = pointLat,
//                    Longitude = pointLon
//                });
//            }
//        }
//        return results;
//    }

//    public async Task<List<GeoResult>> CalculateDistancesToPointsAsync(
//        double lat, double lon,
//        List<(string id, double lat, double lon)> points,
//        GeoUnit unit = GeoUnit.Kilometers,
//        CancellationToken cancellationToken = default)
//    {
//        var tasks = points.Select(async p =>
//        {
//            var distance = await CalculateDistanceAsync(lat, lon, p.lat, p.lon, unit, cancellationToken);
//            return new GeoResult
//            {
//                Id = p.id,
//                Distance = distance,
//                Latitude = p.lat,
//                Longitude = p.lon
//            };
//        });
//        return (await Task.WhenAll(tasks)).ToList();
//    }

//    public async Task<decimal> CalculateDeliveryFeeAsync(double lat1, double lon1, double lat2, double lon2, GeoUnit unit = GeoUnit.Kilometers, CancellationToken cancellationToken = default)
//    {
//        cancellationToken.ThrowIfCancellationRequested();
//        var distance = await CalculateDistanceAsync(lat1, lon1, lat2, lon2, unit, cancellationToken);
//        return (decimal)distance * 0.5m;
//    }

//    public async Task<TimeSpan> CalculateETAAsync(double lat1, double lon1, double lat2, double lon2, double averageSpeedKmH = 30, GeoUnit unit = GeoUnit.Kilometers, CancellationToken cancellationToken = default)
//    {
//        cancellationToken.ThrowIfCancellationRequested();
//        var (distance, duration) = await _mapApiClient.GetRoadDistanceAsync(lat1, lon1, lat2, lon2, unit, cancellationToken);
//        return duration; // Ưu tiên duration từ OSRM nếu có, nếu không dùng ETA tính toán
//    }
//}