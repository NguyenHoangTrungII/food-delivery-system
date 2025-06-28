//using FoodDeliverySystem.Common.Geo.Interfaces;
//using Microsoft.Extensions.Logging;
//using System.Net.Http;
//using System.Text.Json;

//namespace FoodDeliverySystem.Common.Geo.Implementations.Geocoding;

//// NEW: Dịch vụ geocoding với Google Maps
//public class GoogleMapsGeocodingService : IGeocodingService
//{
//    private readonly HttpClient _httpClient;
//    private readonly string _apiKey;
//    private readonly ILogger<GoogleMapsGeocodingService> _logger;

//    public GoogleMapsGeocodingService(HttpClient httpClient, string apiKey, ILogger<GoogleMapsGeocodingService> logger)
//    {
//        _httpClient = httpClient;
//        _apiKey = apiKey;
//        _logger = logger;
//    }

//    public async Task<(double Lat, double Lon)> GeocodeAsync(string address)
//    {
//        var url = $"https://maps.googleapis.com/maps/api/geocode/json?address={Uri.EscapeDataString(address)}&key={_apiKey}";
//        try
//        {
//            var response = await _httpClient.GetAsync(url);
//            response.EnsureSuccessStatusCode();

//            var json = await response.Content.ReadAsStringAsync();
//            var result = JsonSerializer.Deserialize<GoogleGeocodeResponse>(json);

//            var location = result.Results[0].Geometry.Location;
//            return (location.Lat, location.Lng);
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Failed to geocode address: {Address}", address);
//            throw;
//        }
//    }

//    public async Task<string> ReverseGeocodeAsync(double lat, double lon)
//    {
//        var url = $"https://maps.googleapis.com/maps/api/geocode/json?latlng={lat},{lon}&key={_apiKey}";
//        try
//        {
//            var response = await _httpClient.GetAsync(url);
//            response.EnsureSuccessStatusCode();

//            var json = await response.Content.ReadAsStringAsync();
//            var result = JsonSerializer.Deserialize<GoogleGeocodeResponse>(json);

//            return result.Results[0].FormattedAddress;
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Failed to reverse geocode: ({Lat}, {Lon})", lat, lon);
//            throw;
//        }
//    }

//    // Giả định model
//    private class GoogleGeocodeResponse
//    {
//        public List<Result> Results { get; set; }
//        public class Result
//        {
//            public Geometry Geometry { get; set; }
//            public string FormattedAddress { get; set; }
//        }
//        public class Geometry
//        {
//            public Location Location { get; set; }
//        }
//        public class Location
//        {
//            public double Lat { get; set; }
//            public double Lng { get; set; }
//        }
//    }
//}