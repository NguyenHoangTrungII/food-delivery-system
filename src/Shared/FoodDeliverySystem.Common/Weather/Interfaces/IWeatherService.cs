using FoodDeliverySystem.Common.Geo.Models;
using FoodDeliverySystem.Common.Weather.Models;
using System.Threading;
using System.Threading.Tasks;

namespace FoodDeliverySystem.Common.Weather.Interfaces;

public interface IWeatherService
{
    Task<WeatherInfo?> GetCurrentWeatherAsync(
        double latitude,
        double longitude,
        CancellationToken cancellationToken = default);

    Task<WeatherForecast?> GetWeatherForecastAsync(
        double latitude,
        double longitude,
        int hoursAhead,
        CancellationToken cancellationToken = default);
}

