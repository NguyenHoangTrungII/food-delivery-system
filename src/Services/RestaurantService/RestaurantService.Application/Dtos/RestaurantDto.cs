using FoodDeliverySystem.Common.Geo.Models;
using FoodDeliverySystem.Common.Geo.Common.Enums;

namespace RestaurantService.Application.Dtos;

public class RestaurantDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double? Distance { get; set; }
    public decimal? DeliveryFee { get; set; }
    public TimeSpan? ETA { get; set; }
    public string? GeocodedAddress { get; set; }
    public TransportMode? TransportMode { get; set; }
}