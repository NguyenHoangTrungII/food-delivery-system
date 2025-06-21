using FoodDeliverySystem.Common.Geo.Interfaces;
using FoodDeliverySystem.Common.Geo.Models;
using Npgsql;
using System.Threading;
using System.Threading.Tasks;

namespace RestaurantService.Infrastructure.Geo;

public class PostgisGeoDatabaseProvider : IGeoDatabaseProvider
{
    private readonly string _connectionString;

    public PostgisGeoDatabaseProvider(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public async Task<double> CalculateDistanceAsync(double lat1, double lon1, double lat2, double lon2, CancellationToken cancellationToken = default)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        var cmd = new NpgsqlCommand(
            "SELECT ST_Distance(ST_SetSRID(ST_MakePoint(@lon1, @lat1), 4326)::geography, ST_SetSRID(ST_MakePoint(@lon2, @lat2), 4326)::geography)",
            conn);
        cmd.Parameters.AddWithValue("lon1", lon1);
        cmd.Parameters.AddWithValue("lat1", lat1);
        cmd.Parameters.AddWithValue("lon2", lon2);
        cmd.Parameters.AddWithValue("lat2", lat2);
        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        return (double)result;
    }

    public async Task<List<GeoResult>> FindWithinRadiusAsync(double lat, double lon, double radiusInMeters, string tableName, CancellationToken cancellationToken = default)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(cancellationToken);
        var cmd = new NpgsqlCommand(
            $"SELECT id, latitude, longitude, ST_Distance(ST_SetSRID(ST_MakePoint(longitude, latitude), 4326)::geography, ST_SetSRID(ST_MakePoint(@lon, @lat), 4326)::geography) as distance " +
            $"FROM {tableName} WHERE ST_DWithin(ST_SetSRID(ST_MakePoint(longitude, latitude), 4326)::geography, ST_SetSRID(ST_MakePoint(@lon, @lat), 4326)::geography, @radius)",
            conn);
        cmd.Parameters.AddWithValue("lon", lon);
        cmd.Parameters.AddWithValue("lat", lat);
        cmd.Parameters.AddWithValue("radius", radiusInMeters);
        var results = new List<GeoResult>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(new GeoResult
            {
                Id = reader.GetString(0),
                Latitude = reader.GetDouble(1),
                Longitude = reader.GetDouble(2),
                Distance = reader.GetDouble(3)
            });
        }
        return results;
    }
}

//using FoodDeliverySystem.Common.Geo.Interfaces;
//using FoodDeliverySystem.Common.Geo.Models;
//using Npgsql;

//namespace RestaurantService.Infrastructure.Geo;

//public class PostgisGeoDatabaseProvider : IGeoDatabaseProvider
//{
//    private readonly string _connectionString;

//    public PostgisGeoDatabaseProvider(string connectionString)
//    {
//        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
//    }

//    public async Task<double> CalculateDistanceAsync(double lat1, double lon1, double lat2, double lon2)
//    {
//        await using var conn = new NpgsqlConnection(_connectionString);
//        await conn.OpenAsync();
//        var cmd = new NpgsqlCommand(
//            "SELECT ST_Distance(ST_SetSRID(ST_MakePoint(@lon1, @lat1), 4326)::geography, ST_SetSRID(ST_MakePoint(@lon2, @lat2), 4326)::geography)",
//            conn);
//        cmd.Parameters.AddWithValue("lon1", lon1);
//        cmd.Parameters.AddWithValue("lat1", lat1);
//        cmd.Parameters.AddWithValue("lon2", lon2);
//        cmd.Parameters.AddWithValue("lat2", lat2);
//        return (double)await cmd.ExecuteScalarAsync();
//    }

//    public async Task<List<GeoResult>> FindWithinRadiusAsync(double lat, double lon, double radiusInMeters, string tableName)
//    {
//        await using var conn = new NpgsqlConnection(_connectionString);
//        await conn.OpenAsync();
//        var cmd = new NpgsqlCommand(
//            $"SELECT id, latitude, longitude, ST_Distance(ST_SetSRID(ST_MakePoint(longitude, latitude), 4326)::geography, ST_SetSRID(ST_MakePoint(@lon, @lat), 4326)::geography) as distance " +
//            $"FROM {tableName} WHERE ST_DWithin(ST_SetSRID(ST_MakePoint(longitude, latitude), 4326)::geography, ST_SetSRID(ST_MakePoint(@lon, @lat), 4326)::geography, @radius)",
//            conn);
//        cmd.Parameters.AddWithValue("lon", lon);
//        cmd.Parameters.AddWithValue("lat", lat);
//        cmd.Parameters.AddWithValue("radius", radiusInMeters);
//        var results = new List<GeoResult>();
//        await using var reader = await cmd.ExecuteReaderAsync();
//        while (await reader.ReadAsync())
//        {
//            results.Add(new GeoResult
//            {
//                Id = reader.GetString(0),
//                Latitude = reader.GetDouble(1),
//                Longitude = reader.GetDouble(2),
//                Distance = reader.GetDouble(3)
//            });
//        }
//        return results;
//    }
//}