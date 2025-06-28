using FoodDeliverySystem.Common.Geo.Common;
using FoodDeliverySystem.Common.Geo.Common.Enums;
using FoodDeliverySystem.Common.Geo.Interfaces;
using FoodDeliverySystem.Common.Geo.Models;
using Microsoft.Extensions.Logging;

namespace FoodDeliverySystem.Common.Geo.Implementations.DistanceCalculators;

public class PostgisGeoDistanceCalculator : IGeoDistanceCalculator
{
    private readonly IGeoDatabaseProvider _geoDbProvider;
    private readonly ILogger<PostgisGeoDistanceCalculator> _logger;
    private readonly string _geoTableName;

    public PostgisGeoDistanceCalculator(
        IGeoDatabaseProvider geoDbProvider,
        ILogger<PostgisGeoDistanceCalculator> logger,
        string geoTableName = "locations")
    {
        _geoDbProvider = geoDbProvider ?? throw new ArgumentNullException(nameof(geoDbProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _geoTableName = geoTableName;
    }

    public async Task<double> CalculateDistanceAsync(double lat1, double lon1, double lat2, double lon2, GeoUnit unit = GeoUnit.Kilometers, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Calculating distance with PostGIS for ({Lat1}, {Lon1}) to ({Lat2}, {Lon2})", lat1, lon1, lat2, lon2);
        // CHANGED: Sử dụng GeoValidator
        GeoValidator.ValidateCoordinates(lat1, lon1, lat2, lon2);

        var distanceInMeters = await _geoDbProvider.CalculateDistanceAsync(lat1, lon1, lat2, lon2, cancellationToken);
        return unit switch
        {
            GeoUnit.Kilometers => distanceInMeters / 1000,
            GeoUnit.Miles => distanceInMeters / 1609.34,
            _ => distanceInMeters
        };
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
        GeoValidator.ValidateCoordinates(lat, lon);
        var radiusInMeters = radius * (unit == GeoUnit.Kilometers ? 1000 : 1609.34);
        var results = await _geoDbProvider.FindWithinRadiusAsync(lat, lon, radiusInMeters, _geoTableName, cancellationToken);
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

//using Microsoft.Extensions.Logging;
//using FoodDeliverySystem.Common.Geo.Interfaces;
//using FoodDeliverySystem.Common.Geo.Models;
//using System.Threading;
//using Microsoft.Extensions.Options;
//using static FoodDeliverySystem.Common.Geo.Extensions.ServiceCollectionExtensions;
//using FoodDeliverySystem.Common.Geo.Configurations;

//namespace FoodDeliverySystem.Common.Geo.Implementations;

//public class PostgisGeoDistanceCalculator : IGeoDistanceCalculator
//{
//    private readonly IGeoDatabaseProvider _geoDbProvider;
//    private readonly ILogger<PostgisGeoDistanceCalculator> _logger;
//    private readonly string _geoTableName;

//    public PostgisGeoDistanceCalculator(
//        IGeoDatabaseProvider geoDbProvider,
//        ILogger<PostgisGeoDistanceCalculator> logger,
//       IOptions<GeoOptions> geoOptions)
//    {
//        _geoDbProvider = geoDbProvider ?? throw new ArgumentNullException(nameof(geoDbProvider));
//        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//        _geoTableName = geoOptions.Value.PostGIS?.TableName
//        ?? throw new ArgumentNullException(nameof(geoOptions.Value.PostGIS.TableName)); 
//    }

//    public async Task<double> CalculateDistanceAsync(double lat1, double lon1, double lat2, double lon2, GeoUnit unit = GeoUnit.Kilometers, CancellationToken cancellationToken = default)
//    {
//        _logger.LogInformation("Calculating distance with PostGIS for ({Lat1}, {Lon1}) to ({Lat2}, {Lon2})", lat1, lon1, lat2, lon2);
//        try
//        {
//            cancellationToken.ThrowIfCancellationRequested();
//            ValidateCoordinates(lat1, lon1);
//            ValidateCoordinates(lat2, lon2);

//            var distanceInMeters = await _geoDbProvider.CalculateDistanceAsync(lat1, lon1, lat2, lon2, cancellationToken);

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

//    //public async Task<List<GeoResult>> FindWithinRadiusBatchAsync(
//    //double lat, double lon, double radius, List<(string id, double lat, double lon)> points,
//    //GeoUnit unit = GeoUnit.Kilometers, CancellationToken cancellationToken = default)
//    //{
//    //    var radiusInMeters = radius * (unit == GeoUnit.Kilometers ? 1000 : 1609.34);
//    //    return await _geoDbProvider.FindWithinRadiusAsync(lat, lon, radiusInMeters, _geoTableName, cancellationToken);
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

////using Microsoft.Extensions.Logging;
////using FoodDeliverySystem.Common.Geo.Interfaces;
////using FoodDeliverySystem.Common.Geo.Models;
////using System.Runtime.CompilerServices;

////namespace FoodDeliverySystem.Common.Geo.Implementations;

////public class PostgisGeoDistanceCalculator : IGeoDistanceCalculator
////{
////    private readonly IGeoDatabaseProvider _geoDbProvider;
////    private readonly ILogger<PostgisGeoDistanceCalculator> _logger;
////    private readonly string _geoTableName;

////    public PostgisGeoDistanceCalculator(
////        IGeoDatabaseProvider geoDbProvider,
////        ILogger<PostgisGeoDistanceCalculator> logger,
////        string geoTableName = "locations")
////    {
////        _geoDbProvider = geoDbProvider ?? throw new ArgumentNullException(nameof(geoDbProvider));
////        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
////        _geoTableName = geoTableName;
////    }

////    public async Task<double> CalculateDistanceAsync(double lat1, double lon1, double lat2, double lon2, GeoUnit unit = GeoUnit.Kilometers)
////    {
////        _logger.LogInformation("Calculating distance with PostGIS for ({Lat1}, {Lon1}) to ({Lat2}, {Lon2})", lat1, lon1, lat2, lon2);
////        try
////        {
////            ValidateCoordinates(lat1, lon1);
////            ValidateCoordinates(lat2, lon2);

////            var distanceInMeters = await _geoDbProvider.CalculateDistanceAsync(lat1, lon1, lat2, lon2);

////            return unit switch
////            {
////                GeoUnit.Kilometers => distanceInMeters / 1000,
////                GeoUnit.Miles => distanceInMeters / 1609.34,
////                _ => distanceInMeters
////            };
////        }
////        catch (Exception ex)
////        {
////            _logger.LogError(ex, "Error calculating distance for ({Lat1}, {Lon1}) to ({Lat2}, {Lon2})", lat1, lon1, lat2, lon2);
////            throw;
////        }
////    }

////    public async Task<bool> IsWithinRadiusAsync(double lat1, double lon1, double lat2, double lon2, double radius, GeoUnit unit = GeoUnit.Kilometers)
////    {
////        var distance = await CalculateDistanceAsync(lat1, lon1, lat2, lon2, unit);
////        return distance <= radius;
////    }

////    public async Task<List<double>> CalculateDistancesBatchAsync(List<(double lat1, double lon1, double lat2, double lon2)> points, GeoUnit unit = GeoUnit.Kilometers)
////    {
////        var results = new List<double>();
////        foreach (var (lat1, lon1, lat2, lon2) in points)
////        {
////            var distance = await CalculateDistanceAsync(lat1, lon1, lat2, lon2, unit);
////            results.Add(distance);
////        }
////        return results;
////    }

////    public async Task<List<GeoResult>> FindWithinRadiusBatchAsync(double lat, double lon, double radius, List<(double lat, double lon)> points, GeoUnit unit = GeoUnit.Kilometers)
////    {
////        try
////        {
////            ValidateCoordinates(lat, lon);
////            var radiusInMeters = radius * (unit == GeoUnit.Kilometers ? 1000 : 1609.34);

////            var results = await _geoDbProvider.FindWithinRadiusAsync(lat, lon, radiusInMeters, _geoTableName);

////            return results.Select(r => new GeoResult
////            {
////                Id = r.Id,
////                Distance = unit switch
////                {
////                    GeoUnit.Kilometers => r.Distance / 1000,
////                    GeoUnit.Miles => r.Distance / 1609.34,
////                    _ => r.Distance
////                },
////                Latitude = r.Latitude,
////                Longitude = r.Longitude
////            }).ToList();
////        }
////        catch (Exception ex)
////        {
////            _logger.LogError(ex, "Error finding points within radius for ({Lat}, {Lon})", lat, lon);
////            throw;
////        }
////    }

////    public async Task<List<GeoResult>> FindWithinRadiusBatchAsync(
////       double lat,
////       double lon,
////       double radius,
////       List<(double lat, double lon)> points,
////       GeoUnit unit = GeoUnit.Kilometers,
////       CancellationToken cancellationToken = default)
////    {
////        try
////        {
////            ValidateCoordinates(lat, lon);
////            var radiusInMeters = radius * (unit == GeoUnit.Kilometers ? 1000 : 1609.34);

////            var results = await _geoDbProvider.FindWithinRadiusAsync(lat, lon, radiusInMeters, _geoTableName, cancellationToken);

////            return results.Select(r => new GeoResult
////            {
////                Id = r.Id,
////                Distance = unit switch
////                {
////                    GeoUnit.Kilometers => r.Distance / 1000,
////                    GeoUnit.Miles => r.Distance / 1609.34,
////                    _ => r.Distance
////                },
////                Latitude = r.Latitude,
////                Longitude = r.Longitude
////            }).ToList();
////        }
////        catch (OperationCanceledException)
////        {
////            _logger.LogInformation("Operation canceled for finding points within radius ({Lat}, {Lon})", lat, lon);
////            throw;
////        }
////        catch (Exception ex)
////        {
////            _logger.LogError(ex, "Error finding points within radius for ({Lat}, {Lon})", lat, lon);
////            throw;
////        }
////    }

////    public async Task<decimal> CalculateDeliveryFeeAsync(double lat1, double lon1, double lat2, double lon2, GeoUnit unit = GeoUnit.Kilometers)
////    {
////        var distance = await CalculateDistanceAsync(lat1, lon1, lat2, lon2, unit);
////        return (decimal)distance * 0.5m;
////    }

////    public async Task<TimeSpan> CalculateETAAsync(double lat1, double lon1, double lat2, double lon2, double averageSpeedKmH = 30, GeoUnit unit = GeoUnit.Kilometers)
////    {
////        var distance = await CalculateDistanceAsync(lat1, lon1, lat2, lon2, unit);
////        var hours = distance / averageSpeedKmH;
////        return TimeSpan.FromHours(hours);
////    }

////    private void ValidateCoordinates(double lat, double lon)
////    {
////        if (double.IsNaN(lat) || double.IsNaN(lon) || lat < -90 || lat > 90 || lon < -180 || lon > 180)
////            throw new ArgumentException("Invalid coordinates.");
////    }
////}