using FoodDeliverySystem.Common.Geo.Common;
using FoodDeliverySystem.Common.Geo.Common.Enums;
using FoodDeliverySystem.Common.Geo.Interfaces;
using FoodDeliverySystem.Common.Geo.Models;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;

namespace FoodDeliverySystem.Common.Geo.Implementations.DistanceCalculators;

public class MongoGeoDistanceCalculator<T> : IGeoDistanceCalculator where T : class, IGeoEntity
{
    private readonly IGeoDatabaseProvider _dbProvider;
    // NEW: Fallback Haversine
    private readonly HaversineGeoDistanceCalculator _fallbackCalculator;
    private readonly ILogger<MongoGeoDistanceCalculator<T>> _logger;
    private readonly ILogger<HaversineGeoDistanceCalculator> _logger2;


    public MongoGeoDistanceCalculator(
        IGeoDatabaseProvider dbProvider,
        ILogger<MongoGeoDistanceCalculator<T>> logger,
        ILogger<DistanceCalculators.HaversineGeoDistanceCalculator> logger2)
    {
        _dbProvider = dbProvider ?? throw new ArgumentNullException(nameof(dbProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _fallbackCalculator = new HaversineGeoDistanceCalculator(logger2);
    }

    // CHANGED: Sửa lỗi, dùng Haversine thay GeoNear
    public async Task<double> CalculateDistanceAsync(double lat1, double lon1, double lat2, double lon2, GeoUnit unit = GeoUnit.Kilometers, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Calculating distance with MongoDB fallback for ({Lat1}, {Lon1}) to ({Lat2}, {Lon2})", lat1, lon1, lat2, lon2);
        GeoValidator.ValidateCoordinates(lat1, lon1, lat2, lon2);
        return await _fallbackCalculator.CalculateDistanceAsync(lat1, lon1, lat2, lon2, unit, cancellationToken);
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
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            GeoValidator.ValidateCoordinates(lat, lon);
            var radiusInMeters = radius * (unit == GeoUnit.Kilometers ? 1000 : 1609.34);
            var results = await _dbProvider.FindWithinRadiusAsync(lat, lon, radiusInMeters, typeof(T).Name, cancellationToken);
            return results.Select(r => new GeoResult
            {
                Id = r.Id,
                Distance = unit switch
                {
                    GeoUnit.Kilometers => r.Distance / 1000,
                    GeoUnit.Miles => r.Distance / 1609.34,
                    _ => r.Distance
                },
                Latitude = r.Latitude,
                Longitude = r.Longitude
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding points within radius for ({Lat}, {Lon})", lat, lon);
            throw;
        }
    }

    // CHANGED: Sửa lỗi, trả về điểm ngoài bán kính
    public async Task<List<GeoResult>> FindWithoutinRadiusBatchAsync(
        double lat, double lon, double radius, List<(string id, double lat, double lon)> points, GeoUnit unit = GeoUnit.Kilometers,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        GeoValidator.ValidateCoordinates(lat, lon);
        var withinRadius = await FindWithinRadiusBatchAsync(lat, lon, radius, points, unit, cancellationToken);
        var withinIds = withinRadius.Select(r => r.Id).ToHashSet();
        return points.Where(p => !withinIds.Contains(p.id))
            .Select(p => new GeoResult { Id = p.id, Latitude = p.lat, Longitude = p.lon, Distance = null })
            .ToList();
    }

    // CHANGED: Chuyển sang IDeliveryFeeCalculator
    public async Task<decimal> CalculateDeliveryFeeAsync(double lat1, double lon1, double lat2, double lon2, GeoUnit unit = GeoUnit.Kilometers, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Use IDeliveryFeeCalculator instead.");
    }

    // CHANGED: Chuyển sang IETACalculator
    public async Task<TimeSpan> CalculateETAAsync(double lat1, double lon1, double lat2, double lon2, double averageSpeedKmH = 30, GeoUnit unit = GeoUnit.Kilometers, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Use IETACalculator instead.");
    }
}

//using MongoDB.Driver;
//using MongoDB.Driver.GeoJsonObjectModel;
//using Microsoft.Extensions.Logging;
//using FoodDeliverySystem.Common.Geo.Interfaces;
//using FoodDeliverySystem.Common.Geo.Models;
//using System.Threading;

//namespace FoodDeliverySystem.Common.Geo.Implementations;

//public class MongoGeoDistanceCalculator<T> : IGeoDistanceCalculator where T : class, IGeoEntity
//{
//    private readonly IMongoCollection<T> _collection;
//    private readonly ILogger<MongoGeoDistanceCalculator<T>> _logger;

//    public MongoGeoDistanceCalculator(IMongoDatabase db, string collectionName, ILogger<MongoGeoDistanceCalculator<T>> logger)
//    {
//        _collection = db.GetCollection<T>(collectionName);
//        _logger = logger;
//    }

//    public async Task<double> CalculateDistanceAsync(double lat1, double lon1, double lat2, double lon2, GeoUnit unit = GeoUnit.Kilometers, CancellationToken cancellationToken = default)
//    {
//        _logger.LogInformation("Calculating distance with MongoDB for ({Lat1}, {Lon1}) to ({Lat2}, {Lon2})", lat1, lon1, lat2, lon2);
//        try
//        {
//            cancellationToken.ThrowIfCancellationRequested();
//            ValidateCoordinates(lat1, lon1);
//            ValidateCoordinates(lat2, lon2);

//            var point1 = GeoJson.Point(new GeoJson2DGeographicCoordinates(lon1, lat1));

//            var geoNearOptions = new GeoNearOptions<T, T>
//            {
//                DistanceField = nameof(IGeoEntity.Distance),
//                Spherical = true,
//                MaxDistance = 100000 // 100km
//            };

//            var result = await _collection.Aggregate()
//                .GeoNear<T, GeoJson2DGeographicCoordinates, T>(point1, geoNearOptions)
//                .FirstOrDefaultAsync(cancellationToken);

//            var distanceInMeters = result?.Distance ?? throw new InvalidOperationException("Cannot calculate distance.");

//            return unit switch
//            {
//                GeoUnit.Kilometers => distanceInMeters / 1000,
//                GeoUnit.Miles => distanceInMeters / 1609.34,
//                _ => distanceInMeters
//            };
//        }
//        catch (OperationCanceledException)
//        {
//            _logger.LogInformation("Operation canceled for distance calculation ({Lat1}, {Lon1}) to ({Lat2}, {Lon2})", lat1, lon1, lat2, lon2);
//            throw;
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error calculating distance for ({Lat1}, {Lon1}) to ({Lat2}, {Lon2})", lat1, lon1, lat2, lon2);
//            throw;
//        }
//    }

//    public async Task<bool> IsWithinRadiusAsync(double lat1, double lon1, double lat2, double lon2, double radius, GeoUnit unit = GeoUnit.Kilometers, CancellationToken cancellationToken = default)
//    {
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

//    public async Task<List<GeoResult>> FindWithinRadiusBatchAsync(double lat, double lon, double radius, List<(string id, double lat, double lon)> points, GeoUnit unit = GeoUnit.Kilometers, CancellationToken cancellationToken = default)
//    {
//        try
//        {
//            cancellationToken.ThrowIfCancellationRequested();
//            ValidateCoordinates(lat, lon);
//            var point = GeoJson.Point(new GeoJson2DGeographicCoordinates(lon, lat));
//            var radiusInMeters = radius * (unit == GeoUnit.Kilometers ? 1000 : 1609.34);

//            var filter = Builders<T>.Filter.NearSphere(
//                e => new GeoJsonPoint<GeoJson2DGeographicCoordinates>(new GeoJson2DGeographicCoordinates(e.Longitude, e.Latitude)),
//                point,
//                maxDistance: radiusInMeters);

//            var results = await _collection.Find(filter).ToListAsync(cancellationToken);

//            return results.Select(r => new GeoResult
//            {
//                Id = r.Id,
//                Distance = unit switch
//                {
//                    GeoUnit.Kilometers => r.Distance / 1000,
//                    GeoUnit.Miles => r.Distance / 1609.34,
//                    _ => r.Distance
//                },
//                Latitude = r.Latitude,
//                Longitude = r.Longitude
//            }).ToList();
//        }
//        catch (OperationCanceledException)
//        {
//            _logger.LogInformation("Operation canceled for finding points within radius ({Lat}, {Lon})", lat, lon);
//            throw;
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Error finding points within radius for ({Lat}, {Lon})", lat, lon);
//            throw;
//        }
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
//        var distance = await CalculateDistanceAsync(lat1, lon1, lat2, lon2, unit, cancellationToken);
//        return (decimal)distance * 0.5m;
//    }

//    public async Task<TimeSpan> CalculateETAAsync(double lat1, double lon1, double lat2, double lon2, double averageSpeedKmH = 30, GeoUnit unit = GeoUnit.Kilometers, CancellationToken cancellationToken = default)
//    {
//        var distance = await CalculateDistanceAsync(lat1, lon1, lat2, lon2, unit, cancellationToken);
//        var hours = distance / averageSpeedKmH;
//        return TimeSpan.FromHours(hours);
//    }

//    private void ValidateCoordinates(double lat, double lon)
//    {
//        if (double.IsNaN(lat) || double.IsNaN(lon) || lat < -90 || lat > 90 || lon < -180 || lon > 180)
//            throw new ArgumentException("Invalid coordinates.");
//    }
//}