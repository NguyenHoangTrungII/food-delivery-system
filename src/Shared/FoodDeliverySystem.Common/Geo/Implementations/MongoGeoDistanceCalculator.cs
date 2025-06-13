using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using Microsoft.Extensions.Logging;
using FoodDeliverySystem.Common.Geo.Interfaces;
using FoodDeliverySystem.Common.Geo.Models;

namespace FoodDeliverySystem.Common.Geo.Implementations;

public class MongoGeoDistanceCalculator<T> : IGeoDistanceCalculator where T : class, IGeoEntity
{
    private readonly IMongoCollection<T> _collection;
    private readonly ILogger<MongoGeoDistanceCalculator<T>> _logger;

    public MongoGeoDistanceCalculator(IMongoDatabase db, string collectionName, ILogger<MongoGeoDistanceCalculator<T>> logger)
    {
        _collection = db.GetCollection<T>(collectionName);
        _logger = logger;
    }

    public async Task<double> CalculateDistanceAsync(double lat1, double lon1, double lat2, double lon2, GeoUnit unit = GeoUnit.Kilometers)
    {
        _logger.LogInformation("Calculating distance with MongoDB for ({Lat1}, {Lon1}) to ({Lat2}, {Lon2})", lat1, lon1, lat2, lon2);
        try
        {
            ValidateCoordinates(lat1, lon1);
            ValidateCoordinates(lat2, lon2);

            var point1 = GeoJson.Point(new GeoJson2DGeographicCoordinates(lon1, lat1));

            var geoNearOptions = new GeoNearOptions<T, T>
            {
                DistanceField = nameof(IGeoEntity.Distance),
                Spherical = true,
                MaxDistance = 100000 // 100km
            };

            var result = await _collection.Aggregate()
                .GeoNear<T, GeoJson2DGeographicCoordinates, T>(point1, geoNearOptions)
                .FirstOrDefaultAsync();

            var distanceInMeters = result?.Distance ?? throw new InvalidOperationException("Cannot calculate distance.");

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
        try
        {
            ValidateCoordinates(lat, lon);
            var point = GeoJson.Point(new GeoJson2DGeographicCoordinates(lon, lat));
            var radiusInMeters = radius * (unit == GeoUnit.Kilometers ? 1000 : 1609.34);

            var filter = Builders<T>.Filter.NearSphere(
                e => new GeoJsonPoint<GeoJson2DGeographicCoordinates>(new GeoJson2DGeographicCoordinates(e.Longitude, e.Latitude)),
                point,
                maxDistance: radiusInMeters);

            var results = await _collection.Find(filter).ToListAsync();

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

    private void ValidateCoordinates(double lat, double lon)
    {
        if (double.IsNaN(lat) || double.IsNaN(lon) || lat < -90 || lat > 90 || lon < -180 || lon > 180)
            throw new ArgumentException("Invalid coordinates.");
    }
}