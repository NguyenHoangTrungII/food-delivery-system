using FoodDeliverySystem.Common.Geo.Models;
using System.Threading;

namespace FoodDeliverySystem.Common.Geo.Interfaces;

public interface IGeoDatabaseProvider
{
    Task<double> CalculateDistanceAsync(double lat1, double lon1, double lat2, double lon2, CancellationToken cancellationToken = default);
    Task<List<GeoResult>> FindWithinRadiusAsync(double lat, double lon, double radiusInMeters, string tableName, CancellationToken cancellationToken = default);
}