using FoodDeliverySystem.Common.Caching.Services;
using FoodDeliverySystem.Common.Geo.Common;
using FoodDeliverySystem.Common.Geo.Configurations;
using FoodDeliverySystem.Common.Geo.Interfaces;
using FoodDeliverySystem.Common.Geo.Models;
using FoodDeliverySystem.Common.Logging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace FoodDeliverySystem.Common.Geo.Implementations;

public class NominatimGeocodingService : IGeocodingService
{
    private readonly HttpClient _httpClient;
    private readonly NominatimConfig _config;
    private readonly ICacheService _cache;
    private readonly IMemoryCache _localCache;
    private readonly ILoggerAdapter<NominatimGeocodingService> _logger;

    public NominatimGeocodingService(
        HttpClient httpClient,
        IOptions<NominatimConfig> configOptions,
        ICacheService cache,
        IMemoryCache localCache,
        ILoggerAdapter<NominatimGeocodingService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _config = configOptions.Value ?? throw new ArgumentNullException(nameof(configOptions));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _localCache = localCache ?? throw new ArgumentNullException(nameof(localCache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (!_httpClient.DefaultRequestHeaders.UserAgent.Any())
        {
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("FoodDeliverySystem/1.0 (https://localhost)");
        }
    }

    public async Task<(double Lat, double Lon)> GeocodeAsync(string address, CancellationToken cancellationToken = default)
    {
        var geoResult = await GetCoordinatesFromAddressAsync(address, cancellationToken);
        if (geoResult == null)
        {
            throw new Exception("Address not found");
        }
        return (geoResult.Latitude, geoResult.Longitude);
    }

    public async Task<string> ReverseGeocodeAsync(double lat, double lon, CancellationToken cancellationToken = default)
    {
        var geoResult = await GetAddressFromCoordinatesAsync(lat, lon, cancellationToken);
        return geoResult?.Address ?? throw new Exception("Address not found");
    }

    public async Task<GeoResult?> GetAddressFromCoordinatesAsync(double latitude, double longitude, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting address for coordinates ({Latitude}, {Longitude})", latitude, longitude);

        try
        {
            if (!GeoValidator.IsValidCoordinate(latitude, longitude))
            {
                _logger.LogWarning("Invalid coordinates: ({Latitude}, {Longitude})", latitude, longitude);
                return null;
            }

            var lat = Math.Round(latitude, 4);
            var lon = Math.Round(longitude, 4);
            var cacheKey = $"geocoding_coord_{lat}_{lon}_v1";

            // Check local cache
            if (_localCache.TryGetValue(cacheKey, out GeoResult cachedResult))
            {
                _logger.LogInformation("Returning local cached address for ({Latitude}, {Longitude})", lat, lon);
                return cachedResult;
            }

            // Check Redis cache
            cachedResult = await _cache.GetAsync<GeoResult>(cacheKey, cancellationToken);
            if (cachedResult != null)
            {
                _logger.LogInformation("Returning Redis cached address for ({Latitude}, {Longitude})", lat, lon);
                _localCache.Set(cacheKey, cachedResult, _config.LocalCacheTTL);
                return cachedResult;
            }

            // Call Nominatim API
            var url = $"{_config.BaseUrl}/reverse?format=json&lat={lat}&lon={lon}&zoom=18&addressdetails=1";
            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Nominatim API returned status {StatusCode} for ({Latitude}, {Longitude})",
                    response.StatusCode, lat, lon);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var nominatimResponse = JsonSerializer.Deserialize<NominatimResponse>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (nominatimResponse?.DisplayName == null)
            {
                _logger.LogWarning("Invalid Nominatim response for ({Latitude}, {Longitude})", lat, lon);
                return null;
            }

            var geoResult = new GeoResult
            {
                Latitude = lat,
                Longitude = lon,
                Address = nominatimResponse.DisplayName
            };

            await _cache.SetAsync(cacheKey, geoResult, _config.CacheTTL, cancellationToken);
            _localCache.Set(cacheKey, geoResult, _config.LocalCacheTTL);
            _logger.LogInformation("Successfully retrieved and cached address for ({Latitude}, {Longitude})", lat, lon);
            return geoResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get address for ({Latitude}, {Longitude})", latitude, longitude);
            return null;
        }
    }

    public async Task<GeoResult?> GetCoordinatesFromAddressAsync(string address, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting coordinates for address: {Address}", address);

        try
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                _logger.LogWarning("Invalid address provided");
                return null;
            }

            var normalizedAddress = address.Trim().ToLowerInvariant();
            var cacheKey = $"geocoding_addr_{ComputeHash(normalizedAddress)}_v1";

            // Check local cache
            if (_localCache.TryGetValue(cacheKey, out GeoResult cachedResult))
            {
                _logger.LogInformation("Returning local cached coordinates for address: {Address}", address);
                return cachedResult;
            }

            // Check Redis cache
            cachedResult = await _cache.GetAsync<GeoResult>(cacheKey, cancellationToken);
            if (cachedResult != null)
            {
                _logger.LogInformation("Returning Redis cached coordinates for address: {Address}", address);
                _localCache.Set(cacheKey, cachedResult, _config.LocalCacheTTL);
                return cachedResult;
            }

            // Call Nominatim API
            var encodedAddress = Uri.EscapeDataString(normalizedAddress);
            var url = $"{_config.BaseUrl}/search?format=json&q={encodedAddress}&addressdetails=1&limit=1";
            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Nominatim API returned status {StatusCode} for address: {Address}",
                    response.StatusCode, address);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var nominatimResponses = JsonSerializer.Deserialize<NominatimResponse[]>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (nominatimResponses == null || nominatimResponses.Length == 0)
            {
                _logger.LogWarning("Invalid Nominatim response for address: {Address}", address);
                return null;
            }

            var result = nominatimResponses[0];
            if (!double.TryParse(result.Lat, out var latitude) || !double.TryParse(result.Lon, out var longitude))
            {
                _logger.LogWarning("Invalid coordinates in Nominatim response for address: {Address}", address);
                return null;
            }

            var geoResult = new GeoResult
            {
                Latitude = Math.Round(latitude, 4),
                Longitude = Math.Round(longitude, 4),
                Address = result.DisplayName
            };

            await _cache.SetAsync(cacheKey, geoResult, _config.CacheTTL, cancellationToken);
            _localCache.Set(cacheKey, geoResult, _config.LocalCacheTTL);
            _logger.LogInformation("Successfully retrieved and cached coordinates for address: {Address}", address);
            return geoResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get coordinates for address: {Address}", address);
            return null;
        }
    }

    private static string ComputeHash(string input)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash).Replace("/", "_").Replace("+", "-").Replace("=", "");
    }
}

internal class NominatimResponse
{
    public string DisplayName { get; set; } = string.Empty;
    public string Lat { get; set; } = string.Empty;
    public string Lon { get; set; } = string.Empty;
}

public class NominatimConfig
{
    public string BaseUrl { get; set; } = "https://nominatim.openstreetmap.org";
    public TimeSpan CacheTTL { get; set; } = TimeSpan.FromHours(24);
    public TimeSpan LocalCacheTTL { get; set; } = TimeSpan.FromMinutes(5);
}

//using FoodDeliverySystem.Common.Geo.Interfaces;
//using System.Text.Json;

//public class NominatimGeocodingService : IGeocodingService
//{
//    private readonly HttpClient _httpClient;

//    public NominatimGeocodingService(HttpClient httpClient)
//    {
//        _httpClient = httpClient;
//    }

//    public async Task<(double Lat, double Lon)> GeocodeAsync(string address)
//    {
//        var url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(address)}&format=json&limit=1";

//        var response = await _httpClient.GetAsync(url);
//        response.EnsureSuccessStatusCode();

//        var json = await response.Content.ReadAsStringAsync();
//        var result = JsonSerializer.Deserialize<List<NominatimGeocodeResult>>(json);

//        if (result is null || result.Count == 0)
//            throw new Exception("Address not found");

//        return (double.Parse(result[0].Lat), double.Parse(result[0].Lon));
//    }

//    public async Task<string> ReverseGeocodeAsync(double lat, double lon)
//    {
//        var url = $"https://nominatim.openstreetmap.org/reverse?lat={lat}&lon={lon}&format=json";

//        var response = await _httpClient.GetAsync(url);
//        response.EnsureSuccessStatusCode();

//        var json = await response.Content.ReadAsStringAsync();
//        var result = JsonSerializer.Deserialize<NominatimReverseResult>(json);

//        return result.DisplayName;
//    }

//    private class NominatimGeocodeResult
//    {
//        public string Lat { get; set; }
//        public string Lon { get; set; }
//        public string DisplayName { get; set; }
//    }

//    private class NominatimReverseResult
//    {
//        public string DisplayName { get; set; }
//    }
//}
