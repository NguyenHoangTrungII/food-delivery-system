namespace FoodDeliverySystem.Common.Geo.Configurations;

public class ETAConfig
{
    public double AverageSpeedKmH { get; set; } = 30; // Tốc độ trung bình
    public double TrafficFactor { get; set; } = 1.2; // Hệ số giao thông
}