using FoodDeliverySystem.Common.Caching.Services; // NEW
using FoodDeliverySystem.Common.Geo.Common;
using FoodDeliverySystem.Common.Geo.Models;
using FoodDeliverySystem.Common.Logging;
using FoodDeliverySystem.Common.Weather.Configurations;
using FoodDeliverySystem.Common.Weather.Interfaces;
using FoodDeliverySystem.Common.Weather.Models;
using Microsoft.Extensions.Options; // For IOptions
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace FoodDeliverySystem.Common.Weather.Implementations;

public class OpenWeatherService : IWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _baseUrl;
    private readonly ICacheService _cache; // CHANGED: Use ICacheService
    private readonly ILoggerAdapter<OpenWeatherService> _logger;

    public OpenWeatherService(
        HttpClient httpClient,
        IOptions<OpenWeatherConfig> configOptions, // Use IOptions<OpenWeatherConfig>
        ICacheService cache, // CHANGED: Use ICacheService
        ILoggerAdapter<OpenWeatherService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        var config = configOptions.Value ?? throw new ArgumentNullException(nameof(configOptions));
        _apiKey = config.ApiKey
            ?? throw new ArgumentNullException(nameof(config.ApiKey), "OpenWeather API key not configured.");
        _baseUrl = config.BaseUrl
            ?? throw new ArgumentNullException(nameof(config.BaseUrl), "OpenWeather base URL not configured.");
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<WeatherInfo?> GetCurrentWeatherAsync(
        double latitude,
        double longitude,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting current weather for ({Latitude}, {Longitude})", latitude, longitude);

        try
        {
            if (!GeoValidator.IsValidCoordinate(latitude, longitude))
            {
                _logger.LogWarning("Invalid coordinates: ({Latitude}, {Longitude})", latitude, longitude);
                return null;
            }

            var cacheKey = $"weather_{latitude}_{longitude}";
            var cachedWeather = await _cache.GetAsync<WeatherInfo>(cacheKey, cancellationToken); // CHANGED: Use ICacheService
            if (cachedWeather != null)
            {
                _logger.LogInformation("Returning cached weather for ({Latitude}, {Longitude})", latitude, longitude);
                return cachedWeather;
            }

            var url = $"{_baseUrl}/weather?lat={latitude}&lon={longitude}&appid={_apiKey}&units=metric";
            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("OpenWeather API returned status {StatusCode} for ({Latitude}, {Longitude})",
                    response.StatusCode, latitude, longitude);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var weatherResponse = JsonSerializer.Deserialize<OpenWeatherResponse>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (weatherResponse?.Weather == null || weatherResponse.Main == null)
            {
                _logger.LogWarning("Invalid OpenWeather response for ({Latitude}, {Longitude})", latitude, longitude);
                return null;
            }

            var weatherInfo = new WeatherInfo
            {
                Condition = weatherResponse.Weather.FirstOrDefault()?.Main ?? "Unknown",
                Temperature = weatherResponse.Main.Temp,
                Precipitation = weatherResponse.Rain?.OneHour ?? 0,
                WindSpeed = weatherResponse.Wind.Speed * 3.6 // m/s to km/h
            };

            await _cache.SetAsync(cacheKey, weatherInfo, TimeSpan.FromMinutes(10), cancellationToken); // CHANGED: Use ICacheService
            _logger.LogInformation("Successfully retrieved weather for ({Latitude}, {Longitude})", latitude, longitude);
            return weatherInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get weather for ({Latitude}, {Longitude})", latitude, longitude);
            return null;
        }
    }

    public async Task<WeatherForecast?> GetWeatherForecastAsync(
        double latitude,
        double longitude,
        int hoursAhead,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting weather forecast for ({Latitude}, {Longitude}) for {HoursAhead} hours",
            latitude, longitude, hoursAhead);
        return await Task.FromResult<WeatherForecast?>(null); // For future paid API
    }
}

