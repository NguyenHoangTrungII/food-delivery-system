using FoodDeliverySystem.Common.Geo.Configurations;
using FoodDeliverySystem.Common.Geo.Models;

namespace FoodDeliverySystem.Common.Geo.Interfaces;

public interface IETACalculator
{
    Task<TimeSpan> CalculateETAAsync(double lat1, double lon1, double lat2, double lon2, ETAConfig config, GeoUnit unit);
}