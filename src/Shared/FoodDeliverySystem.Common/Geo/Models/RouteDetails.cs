
using FoodDeliverySystem.Common.Geo.Common.Enums;

namespace FoodDeliverySystem.Common.Geo.Models;

public class RouteDetails
{
    public double Distance { get; set; }
    public TimeSpan Duration { get; set; }
    public List<GeoPoint> Polyline { get; set; }
    public TrafficStatus TrafficStatus { get; set; }
}