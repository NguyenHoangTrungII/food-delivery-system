using FoodDeliverySystem.Common.Geo.Common.Enums;
using FoodDeliverySystem.Common.Geo.Configurations;
using FoodDeliverySystem.Common.Geo.Interfaces;
using FoodDeliverySystem.Common.Geo.Models;
using FoodDeliverySystem.Common.Geo.Services;
using FoodDeliverySystem.Common.Weather.Interfaces;

namespace FoodDeliverySystem.Common.Geo.Implementations.FeeCalculators;

// NEW: Triển khai tính phí giao hàng
public class DynamicFeeCalculator : IDeliveryFeeCalculator
{
    private readonly IGeoDistanceCalculator _calculator;
    private readonly IWeatherService _weatherService;

    public DynamicFeeCalculator(IGeoDistanceCalculator calculator, IWeatherService weatherService)
    {
        _calculator = calculator;
        _weatherService = weatherService;
    }

    public async Task<decimal> CalculateFeeAsync(double lat1, double lon1, double lat2, double lon2, FeeConfig config, GeoUnit unit = GeoUnit.Kilometers)
    {
        var distance = await _calculator.CalculateDistanceAsync(lat1, lon1, lat2, lon2, unit);
        var fee = config.BaseFee + (decimal)distance * config.PerKmFee;

        // NEW: Phí giờ cao điểm
        var now = DateTime.Now.TimeOfDay;
        foreach (var surge in config.SurgePricing)
        {
            if (now >= surge.Key && now < surge.Key.Add(TimeSpan.FromHours(2)))
            {
                fee += surge.Value;
            }
        }

        // NEW: Phí thời tiết
        var weather = await _weatherService.GetCurrentWeatherAsync(lat1, lon1);
        if (weather != null && (weather.Condition.Contains("Rain", StringComparison.OrdinalIgnoreCase) ||
                                weather.Condition.Contains("Storm", StringComparison.OrdinalIgnoreCase)))
        {
            fee += config.WeatherSurcharge;
        }

        return fee;
    }
}