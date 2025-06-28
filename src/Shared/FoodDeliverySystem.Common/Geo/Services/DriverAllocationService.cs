using FoodDeliverySystem.Common.Geo.Common;
using FoodDeliverySystem.Common.Geo.Common.Enums;
using FoodDeliverySystem.Common.Geo.Interfaces;
using FoodDeliverySystem.Common.Geo.Models;

namespace FoodDeliverySystem.Common.Geo.Services;

// NEW: Dịch vụ phân bổ tài xế
public class DriverAllocationService
{
    private readonly IGeoDistanceCalculator _calculator;

    public DriverAllocationService(IGeoDistanceCalculator calculator)
    {
        _calculator = calculator;
    }

    public async Task<List<GeoResult>> FindNearestDriversAsync(
        double lat, double lon, double radius, List<Driver> drivers, GeoUnit unit = GeoUnit.Kilometers)
    {
        var availableDrivers = drivers.Where(d => d.Status == DriverStatus.Available && d.VehicleType == TransportMode.Bike).ToList();
        var points = availableDrivers.Select(d => (d.Id, d.Latitude, d.Longitude)).ToList();
        return await _calculator.FindWithinRadiusBatchAsync(lat, lon, radius, points, unit);
    }
}