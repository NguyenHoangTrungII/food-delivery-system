using FoodDeliverySystem.Common.Geo.Common.Constants;
using FoodDeliverySystem.Common.Geo.Common.Enums;
using FoodDeliverySystem.Common.Geo.Configurations;
using FoodDeliverySystem.Common.Geo.Interfaces;
using FoodDeliverySystem.Common.Geo.Models;

namespace FoodDeliverySystem.Common.Geo.Implementations.ETACalculators;

// NEW: Triển khai tính ETA
public class DynamicETACalculator : IETACalculator
{
    private readonly IGeoDistanceCalculator _calculator;

    public DynamicETACalculator(IGeoDistanceCalculator calculator)
    {
        _calculator = calculator;
    }

    public async Task<TimeSpan> CalculateETAAsync(double lat1, double lon1, double lat2, double lon2, ETAConfig config, GeoUnit unit = GeoUnit.Kilometers)
    {
        var distance = await _calculator.CalculateDistanceAsync(lat1, lon1, lat2, lon2, unit);
        var hours = distance / config.AverageSpeedKmH * config.TrafficFactor;
        return TimeSpan.FromHours(hours);
    }
}