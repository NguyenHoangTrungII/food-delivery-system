using FoodDeliverySystem.Common.Geo.Common.Enums;
using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace FoodDeliverySystem.Common.Geo.Services;

// NEW: Dịch vụ thời tiết
public class WeatherService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<WeatherService> _logger;

    public WeatherService(HttpClient httpClient, string apiKey, ILogger<WeatherService> logger)
    {
        _httpClient = httpClient;
        _apiKey = apiKey;
        _logger = logger;
    }

    public async Task<WeatherCondition> GetWeatherAsync(double lat, double lon)
    {
        var url = $"https://api.openweathermap.org/data/2.5/weather?lat={lat}&lon={lon}&appid={_apiKey}";
        try
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            // Giả định parse JSON để lấy thời tiết
            return WeatherCondition.Clear; // Cần parse thực tế
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get weather for ({Lat}, {Lon})", lat, lon);
            return WeatherCondition.Clear; // Fallback
        }
    }
}