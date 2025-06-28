using FoodDeliverySystem.Common.Geo.Common.Enums;
using FoodDeliverySystem.Common.Geo.Configurations;
using FoodDeliverySystem.Common.Geo.Models;

namespace FoodDeliverySystem.Common.Geo.Interfaces;

// NEW: Giao diện cho tính phí giao hàng
public interface IDeliveryFeeCalculator
{
    Task<decimal> CalculateFeeAsync(double lat1, double lon1, double lat2, double lon2, FeeConfig config, GeoUnit unit = GeoUnit.Kilometers);
}