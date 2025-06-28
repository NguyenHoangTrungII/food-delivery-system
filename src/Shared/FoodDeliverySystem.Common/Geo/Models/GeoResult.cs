namespace FoodDeliverySystem.Common.Geo.Models;

public class GeoResult
{
    public string Id { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double? Distance { get; set; }
    public string Address { get; set; }
}