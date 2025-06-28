using FoodDeliverySystem.Common.Geo.Common;
using FoodDeliverySystem.Common.Geo.Common.Enums;
using FoodDeliverySystem.Common.Geo.Interfaces;
using FoodDeliverySystem.Common.Geo.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text.Json;

namespace FoodDeliverySystem.Common.Geo.Implementations.MapApiClients;

public class OsrmApiClient : IMapApiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _osrmUrl;
    private readonly ILogger<OsrmApiClient> _logger;

    public OsrmApiClient(HttpClient httpClient, IConfiguration configuration, ILogger<OsrmApiClient> logger)
    {
        _httpClient = httpClient;
        _osrmUrl = configuration["Geo:OSRM:Url"] ?? "http://localhost:5000";
        _logger = logger;
    }

    public async Task<(double Distance, TimeSpan Duration)> GetRoadDistanceAsync(
        double lat1, double lon1, double lat2, double lon2, GeoUnit unit = GeoUnit.Kilometers, CancellationToken cancellationToken = default)
    {
        var url = $"{_osrmUrl}/route/v1/driving/{lon1},{lat1};{lon2},{lat2}?overview=false";
        try
        {
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<OsrmRouteResponse>(json, options);

            if (result?.Code != "Ok")
            {
                throw new Exception($"OSRM API error: {result?.Message ?? "Unknown"}");
            }

            var distanceMeters = result.Routes[0].Distance;
            var durationSeconds = result.Routes[0].Duration;

            double distance = unit switch
            {
                GeoUnit.Kilometers => distanceMeters / 1000.0,
                GeoUnit.Miles => distanceMeters / 1609.34,
                _ => distanceMeters
            };

            return (distance, TimeSpan.FromSeconds(durationSeconds));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get route from OSRM: {Url}", url);
            throw;
        }
    }

    // NEW: Hỗ trợ lộ trình chi tiết
    public async Task<RouteDetails> GetRouteDetailsAsync(double lat1, double lon1, double lat2, double lon2, TransportMode mode, CancellationToken cancellationToken)
    {
        var profile = mode switch
        {
            TransportMode.Bike => "bike",
            TransportMode.Foot => "foot",
            _ => "driving"
        };
        var url = $"{_osrmUrl}/route/v1/{profile}/{lon1},{lat1};{lon2},{lat2}?overview=full";
        try
        {
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var result = JsonSerializer.Deserialize<OsrmRouteResponse>(json, options);

            if (result?.Code != "Ok")
            {
                throw new Exception($"OSRM API error: {result?.Message ?? "Unknown"}");
            }

            var distanceMeters = result.Routes[0].Distance;
            var durationSeconds = result.Routes[0].Duration;
            // Giả định polyline được decode từ geometry
            var polyline = new List<GeoPoint>(); // Cần decode geometry từ OSRM

            return new RouteDetails
            {
                Distance = distanceMeters / 1000.0,
                Duration = TimeSpan.FromSeconds(durationSeconds),
                Polyline = polyline,
                TrafficStatus = TrafficStatus.Light // OSRM không hỗ trợ, mặc định
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get route details from OSRM: {Url}", url);
            throw;
        }
    }
}

//using System;
//using System.Net.Http;
//using System.Text.Json;
//using System.Threading;
//using System.Threading.Tasks;
//using FoodDeliverySystem.Common.Geo.Interfaces;
//using FoodDeliverySystem.Common.Geo.Models;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Logging;

//namespace FoodDeliverySystem.Common.Geo.Implementations
//{
//    public class OsrmApiClient : IMapApiClient
//    {
//        private readonly HttpClient _httpClient;
//        private readonly string _osrmUrl;
//        private readonly ILogger<OsrmApiClient> _logger;

//        public OsrmApiClient(HttpClient httpClient, IConfiguration configuration, ILogger<OsrmApiClient> logger)
//        {
//            _httpClient = httpClient;
//            //_osrmUrl = configuration["Geo:OSRM:Url"] ?? "http://localhost:5000";
//            _osrmUrl =  "http://localhost:5000";

//            _logger = logger;
//        }

//        public async Task<(double Distance, TimeSpan Duration)> GetRoadDistanceAsync(
//            double lat1, double lon1, double lat2, double lon2,
//            GeoUnit unit = GeoUnit.Kilometers,
//            CancellationToken cancellationToken = default)
//        {
//            var url = $"{_osrmUrl}/route/v1/driving/{lon1},{lat1};{lon2},{lat2}?overview=false";
//            try
//            {
//                var response = await _httpClient.GetAsync(url, cancellationToken);
//                response.EnsureSuccessStatusCode();

//                var json = await response.Content.ReadAsStringAsync(cancellationToken);

//                var options = new JsonSerializerOptions
//                {
//                    PropertyNameCaseInsensitive = true
//                };

//                var result = JsonSerializer.Deserialize<OsrmRouteResponse>(json, options);
//                //var result = JsonSerializer.Deserialize<OsrmRouteResponse>(json);

//                if (result?.Code != "Ok")
//                {
//                    _logger.LogError("OSRM API error: {Message}", result?.Message ?? "No response");
//                    throw new InvalidOperationException($"OSRM API error: {result?.Message ?? "Unknown"}");
//                }

//                var distanceMeters = result.Routes[0].Distance;
//                var durationSeconds = result.Routes[0].Duration;

//                double distance = unit switch
//                {
//                    GeoUnit.Kilometers => distanceMeters / 1000.0,
//                    GeoUnit.Miles => distanceMeters / 1609.34,
//                    _ => distanceMeters
//                };

//                return (distance, TimeSpan.FromSeconds(durationSeconds));
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Failed to get route from OSRM: {Url}", url);
//                throw;
//            }
//        }
//    }

//    //public class OsrmRouteResponse
//    //{
//    //    public string Code { get; set; }
//    //    public string Message { get; set; }
//    //    public List<Route> Routes { get; set; }

//    //    public class Route
//    //    {
//    //        public double Distance { get; set; } // Mét
//    //        public double Duration { get; set; } // Giây
//    //    }
//    //}




//}