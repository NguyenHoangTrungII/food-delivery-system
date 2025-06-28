using FoodDeliverySystem.Common.Caching.Services;
using FoodDeliverySystem.Common.Geo.Common;
using FoodDeliverySystem.Common.Geo.Common.Enums;
using FoodDeliverySystem.Common.Geo.Interfaces;
using FoodDeliverySystem.Common.Geo.Models;
using Microsoft.Extensions.Logging;
using Prometheus;
using System.Diagnostics.Metrics;

namespace FoodDeliverySystem.Common.Geo.Implementations.DistanceCalculators;

public class CachedGeoDistanceCalculator : IGeoDistanceCalculator
{
    private readonly ICacheService _cache;
    private readonly IGeoDistanceCalculator _innerCalculator;
    private readonly IMapApiClient _mapApiClient;
    private readonly ILogger<CachedGeoDistanceCalculator> _logger;
    private readonly TimeSpan _cacheDuration;
    // NEW: Metric giám sát
    private static readonly Counter _cacheHitCounter = Metrics.CreateCounter("geo_cache_hits", "Cache hits for geo calculations");
    private static readonly Histogram _calculationDuration = Metrics.CreateHistogram("geo_calculation_duration", "Duration of geo calculations");

    public CachedGeoDistanceCalculator(
        ICacheService cache,
        IGeoDistanceCalculator innerCalculator,
        IMapApiClient mapApiClient,
        ILogger<CachedGeoDistanceCalculator> logger,
        TimeSpan? cacheDuration = null)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _innerCalculator = innerCalculator ?? throw new ArgumentNullException(nameof(innerCalculator));
        _mapApiClient = mapApiClient ?? throw new ArgumentNullException(nameof(mapApiClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cacheDuration = cacheDuration ?? TimeSpan.FromMinutes(30);
    }

    public async Task<double> CalculateDistanceAsync(double lat1, double lon1, double lat2, double lon2, GeoUnit unit = GeoUnit.Kilometers, CancellationToken cancellationToken = default)
    {
        using var timer = _calculationDuration.NewTimer();
        // CHANGED: Sử dụng GeoValidator
        GeoValidator.ValidateCoordinates(lat1, lon1, lat2, lon2);
        // NEW: Phân vùng cache theo lưới tọa độ
        var cacheKey = GetCacheKey(lat1, lon1, lat2, lon2, unit);
        var cachedDistance = await _cache.GetAsync<double?>(cacheKey, cancellationToken);

        if (cachedDistance.HasValue)
        {
            _cacheHitCounter.Inc();
            _logger.LogInformation("Cache hit for distance calculation: {CacheKey}", cacheKey);
            return cachedDistance.Value;
        }

        var distance = await _innerCalculator.CalculateDistanceAsync(lat1, lon1, lat2, lon2, unit, cancellationToken);
        await _cache.SetAsync(cacheKey, distance, _cacheDuration, cancellationToken);
        _logger.LogInformation("Cache miss, calculated and cached distance: {CacheKey}", cacheKey);
        return distance;
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
                return new GeoResult
                {
                    Id = p.id,
                    Distance = distance,
                    Latitude = p.lat,
                    Longitude = p.lon
                };
            }
            return null;
        });
        var geoResults = await Task.WhenAll(tasks);
        return geoResults.Where(r => r != null).ToList();
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
                return new GeoResult
                {
                    Id = p.id,
                    Distance = distance,
                    Latitude = p.lat,
                    Longitude = p.lon
                };
            }
            return null;
        });
        var geoResults = await Task.WhenAll(tasks);
        return geoResults.Where(r => r != null).ToList();
    }

    // NEW: Phân vùng cache theo lưới tọa độ
    private string GetCacheKey(double lat1, double lon1, double lat2, double lon2, GeoUnit unit)
    {
        var gridLat1 = Math.Round(lat1, 2);
        var gridLon1 = Math.Round(lon1, 2);
        var gridLat2 = Math.Round(lat2, 2);
        var gridLon2 = Math.Round(lon2, 2);
        return $"Geo:Distance_{gridLat1}_{gridLon1}_{gridLat2}_{gridLon2}_{unit}";
    }
}

//using FoodDeliverySystem.Common.Caching.Services;
//using FoodDeliverySystem.Common.Geo.Interfaces;
//using FoodDeliverySystem.Common.Geo.Models;
//using Microsoft.Extensions.Logging;
//using System.Threading;

//namespace FoodDeliverySystem.Common.Geo.Implementations;

//public class CachedGeoDistanceCalculator : IGeoDistanceCalculator
//{
//    private readonly ICacheService _cache;
//    private readonly IGeoDistanceCalculator _innerCalculator;
//    private readonly IMapApiClient _mapApiClient;
//    private readonly ILogger<CachedGeoDistanceCalculator> _logger;
//    private readonly TimeSpan _cacheDuration;

//    public CachedGeoDistanceCalculator(
//        ICacheService cache,
//        IGeoDistanceCalculator innerCalculator,
//        IMapApiClient mapApiClient,
//        ILogger<CachedGeoDistanceCalculator> logger,
//        TimeSpan? cacheDuration = null)
//    {
//        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
//        _innerCalculator = innerCalculator ?? throw new ArgumentNullException(nameof(innerCalculator));
//        _mapApiClient = mapApiClient ?? throw new ArgumentNullException(nameof(mapApiClient));
//        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//        _cacheDuration = cacheDuration ?? TimeSpan.FromMinutes(30);
//    }

//    public async Task<double> CalculateDistanceAsync(double lat1, double lon1, double lat2, double lon2, GeoUnit unit = GeoUnit.Kilometers, CancellationToken cancellationToken = default)
//    {
//        var cacheKey = $"Geo:Distance_{lat1}_{lon1}_{lat2}_{lon2}_{unit}";
//        var cachedDistance = await _cache.GetAsync<double?>(cacheKey, cancellationToken);

//        if (cachedDistance.HasValue)
//        {
//            _logger.LogInformation("Cache hit for distance calculation: {CacheKey}", cacheKey);
//            return cachedDistance.Value;
//        }

//        var distance = await _innerCalculator.CalculateDistanceAsync(lat1, lon1, lat2, lon2, unit, cancellationToken);
//        await _cache.SetAsync(cacheKey, distance, _cacheDuration, cancellationToken);
//        _logger.LogInformation("Cache miss, calculated and cached: {CacheKey}", cacheKey);
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

//    //public async Task<List<GeoResult>> FindWithinRadiusBatchAsync(double lat, double lon, double radius, List<(double lat, double lon)> points, GeoUnit unit = GeoUnit.Kilometers, CancellationToken cancellationToken = default)
//    //{
//    //    cancellationToken.ThrowIfCancellationRequested();
//    //    var results = new List<GeoResult>();
//    //    foreach (var (pointLat, pointLon) in points)
//    //    {
//    //        cancellationToken.ThrowIfCancellationRequested();
//    //        var distance = await CalculateDistanceAsync(lat, lon, pointLat, pointLon, unit, cancellationToken);
//    //        //if (distance <= radius)
//    //        //{
//    //            results.Add(new GeoResult { Id = Guid.NewGuid().ToString(), Distance = distance, Latitude = pointLat, Longitude = pointLon });
//    //        //}
//    //    }
//    //    return results;
//    //}

//    public async Task<List<GeoResult>> FindWithinRadiusBatchAsync(
//    double lat,
//    double lon,
//    double radius,
//    List<(string id, double lat, double lon)> points,
//    GeoUnit unit = GeoUnit.Kilometers,
//    CancellationToken cancellationToken = default)
//    {
//        cancellationToken.ThrowIfCancellationRequested();
//        var results = new List<GeoResult>();
//        foreach (var (id, pointLat, pointLon) in points)
//        {
//            cancellationToken.ThrowIfCancellationRequested();
//            var distance = await CalculateDistanceAsync(lat, lon, pointLat, pointLon, unit, cancellationToken);
//            //if (distance <= radius) // Bỏ comment để chỉ trả về các điểm trong bán kính
//            //{
//                results.Add(new GeoResult
//                {
//                    Id = id, // Sử dụng id của nhà hàng
//                    Distance = distance,
//                    Latitude = pointLat,
//                    Longitude = pointLon
//                });
//            //}
//        }
//        return results;
//    }

//    public async Task<List<GeoResult>> CalculateDistancesToPointsAsync(
//    double lat, double lon,
//    List<(string id, double lat, double lon)> points,
//    GeoUnit unit = GeoUnit.Kilometers,
//    CancellationToken cancellationToken = default)
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
//        var distance = await CalculateDistanceAsync(lat1, lon1, lat2, lon2, unit, cancellationToken);
//        var hours = distance / averageSpeedKmH;
//        return TimeSpan.FromHours(hours);
//    }
//}