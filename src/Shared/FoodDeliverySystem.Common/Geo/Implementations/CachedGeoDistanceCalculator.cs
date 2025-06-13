using FoodDeliverySystem.Common.Caching.Services;
using FoodDeliverySystem.Common.Geo.Interfaces;
using FoodDeliverySystem.Common.Geo.Models;
using Microsoft.Extensions.Logging;

namespace FoodDeliverySystem.Common.Geo.Implementations;

public class CachedGeoDistanceCalculator : IGeoDistanceCalculator
{
    private readonly ICacheService _cache;
    private readonly IGeoDistanceCalculator _innerCalculator;
    private readonly ILogger<CachedGeoDistanceCalculator> _logger;
    private readonly TimeSpan _cacheDuration;

    public CachedGeoDistanceCalculator(
        ICacheService cache,
        IGeoDistanceCalculator innerCalculator,
        ILogger<CachedGeoDistanceCalculator> logger,
        TimeSpan? cacheDuration = null)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _innerCalculator = innerCalculator ?? throw new ArgumentNullException(nameof(innerCalculator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cacheDuration = cacheDuration ?? TimeSpan.FromMinutes(30); // Mặc định 30 phút
    }

    public async Task<double> CalculateDistanceAsync(double lat1, double lon1, double lat2, double lon2, GeoUnit unit = GeoUnit.Kilometers)
    {
        var cacheKey = $"Distance_{lat1}_{lon1}_{lat2}_{lon2}_{unit}";
        var cachedDistance = await _cache.GetAsync<double?>(cacheKey);
        if (cachedDistance.HasValue)
        {
            _logger.LogInformation("Cache hit for distance calculation: {CacheKey}", cacheKey);
            return cachedDistance.Value;
        }

        var distance = await _innerCalculator.CalculateDistanceAsync(lat1, lon1, lat2, lon2, unit);
        await _cache.SetAsync(cacheKey, distance, _cacheDuration);
        _logger.LogInformation("Cache miss, calculated and cached: {CacheKey}", cacheKey);
        return distance;
    }

    public async Task<bool> IsWithinRadiusAsync(double lat1, double lon1, double lat2, double lon2, double radius, GeoUnit unit = GeoUnit.Kilometers)
    {
        return await _innerCalculator.IsWithinRadiusAsync(lat1, lon1, lat2, lon2, radius, unit);
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
        return await _innerCalculator.FindWithinRadiusBatchAsync(lat, lon, radius, points, unit);
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
}